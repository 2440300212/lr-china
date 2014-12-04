
using System;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Linq;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;
using System.Collections.Generic;
namespace Executed
{
    class Program
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        private static readonly Obj_AI_Hero me = ObjectManager.Player;
        public static Spell Q, W, E, R, P;
        public static float currEnergy;
        public static bool haspassive = true;
        public static bool PingCasted = false;
        public static float eEnergy= 120f+5f;
        public static Items.Item botrk = new Items.Item(3153, 450);
        public static Items.Item bilgewater = new Items.Item(3144, 450);
        public static Items.Item hexgun = new Items.Item(3146, 700);
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;


        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (me.BaseSkinName != "Shen") return;
            InitMenu();
            InitShen();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Shen</font>");
            //Game.OnGameInput += Game_GameInput;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Obj_AI_Base.OnCreate += OnCreate;
            //Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;


        }

        private static void InitShen()
        {
            Q = new Spell(SpellSlot.Q, 475);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 600);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R, float.MaxValue);
            P = new Spell(me.GetSpellSlot("ShenKiAttack", false));//Doesn't Work
            Q.SetTargetted(0.5f, 1500f);
            E.SetSkillshot(0.25f, 100f, 1750f, false, SkillshotType.SkillshotLine);
        }
         private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs Spell)
        {
         }
        private static void InitMenu()
        {
            config = new Menu("Soresu-Shen", "SRS_Shen", true);
            // Target Selector
            Menu menuTS = new Menu("Selector", "tselect");
            SimpleTs.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            Menu menuOrb = new Menu("Orbwalker", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            Menu menuK = new Menu("Keybinds", "demkeys");
            menuK.AddItem(new MenuItem("combokey", "Combo Key")).SetValue(new KeyBind(32, KeyBindType.Press));
            menuK.AddItem(new MenuItem("harasskey", "Harass Key")).SetValue(new KeyBind(67, KeyBindType.Press));
            menuK.AddItem(new MenuItem("clearkey", "Clear Key")).SetValue(new KeyBind(86, KeyBindType.Press));
            config.AddSubMenu(menuK);

            // Draw settings
            Menu menuD = new Menu("Drawings ", "dsettings");
            menuD.AddItem(new MenuItem("dsep1", "---Drawing Settings---"));
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(true, Color.FromArgb(80, 150, 62, 172)));
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range")).SetValue(new Circle(true, Color.FromArgb(80, 150, 62, 172)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(true, Color.FromArgb(80, 150, 62, 172)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);

            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("csep1", "---Q Settings---"));
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);

            menuC.AddItem(new MenuItem("csep2", "---W Settings---"));
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);

            menuC.AddItem(new MenuItem("csep3", "---E Settings---"));
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("useemin", "Try to use E min")).SetValue(new Slider(1, 1, 5));
            menuC.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddSubMenu(menuC);

            // Misc Settings
            Menu menuU = new Menu("Misc ", "usettings");
            menuU.AddItem(new MenuItem("csep4", "---Q Settings---"));
            menuU.AddItem(new MenuItem("harassq", "Harass with Q")).SetValue(true);
            menuU.AddItem(new MenuItem("harassqwithe", "Keep energy for E")).SetValue(true);
            menuU.AddItem(new MenuItem("autoqls", "Lasthit with Q")).SetValue(true);
            menuU.AddItem(new MenuItem("autoqwithe", "Keep energy for E")).SetValue(true);
            menuU.AddItem(new MenuItem("csep5", "---W Settings---"));
            menuU.AddItem(new MenuItem("autow", "Try to block non-skillshot spells")).SetValue(true);
            menuU.AddItem(new MenuItem("wabove", "Min damage in shield %")).SetValue(new Slider(50, 0, 100));
            menuU.AddItem(new MenuItem("autowwithe", "Keep energy for E")).SetValue(true);
            menuU.AddItem(new MenuItem("csep51", "---E Settings---"));
            menuU.AddItem(new MenuItem("useeagc", "Use E to anti gap closer")).SetValue(false);
            menuU.AddItem(new MenuItem("useeint", "Use E to interrupt")).SetValue(true);
            menuU.AddItem(new MenuItem("csep6", "---Ult Settings---"));
            menuU.AddItem(new MenuItem("user", "Use R")).SetValue(true);
            menuU.AddItem(new MenuItem("atpercent", "Friend under")).SetValue(new Slider(20, 0, 100));
            config.AddSubMenu(menuU);
            var sulti = new Menu("Don't ult on ", "dontult");
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                if (hero.SkinName!= me.SkinName)
                sulti.AddItem(new MenuItem("ult" + hero.SkinName, hero.SkinName)).SetValue(false);
            }
            config.AddSubMenu(sulti);
            
            config.AddToMainMenu();
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            
        }

        private static void Game_OnDraw(EventArgs args)
        {
            DrawCircle("drawaa", me.AttackRange);
            DrawCircle("drawqq", Q.Range);
            DrawCircle("drawee", E.Range);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();

            
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            
        }

        private static void Game_GameInput(GameInputEventArgs args)
        {


        }
        static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {

        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            GetPassive();
            currEnergy = me.Mana;
            //Game.PrintChat(P.IsReady().ToString());
             bool minionBlock = false;
            foreach (Obj_AI_Minion minion in MinionManager.GetMinions(me.Position, me.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(me, minion, false))
                    minionBlock = true;
            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (!minionBlock) Harass();
                    Ulti();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LasthitQ();
                    Clear();
                    Ulti();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LasthitQ();
                    Ulti();
                    break;
                default:
                    if (!minionBlock)
                    {
                        Ulti();
                        //AutoQ();
                        //AutoW();
                    }
                    break;
            }
        }

        private static void GetPassive()
        {
            var has=false;
            foreach (BuffInstance buff in me.Buffs)
            {
                if (buff.Name == "shenwayoftheninjaaura")
                {
                    has = true;
                }
            }
            if(has){
                  haspassive = true;
            }else{
                  haspassive = false;
            }
                   
        }
        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("useeagc").GetValue<bool>()) return;
            if (gapcloser.Sender.IsValidTarget(E.Range) && E.IsReady() && me.Distance(gapcloser.Sender) < 400) E.Cast(gapcloser.Sender.Position + Vector3.Normalize(gapcloser.Sender.Position - me.Position) * 200, config.Item("packets").GetValue<bool>());
        }
        private static void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!config.Item("useeint").GetValue<bool>()) return;
            if (unit.IsValidTarget(E.Range) && E.IsReady()) E.Cast(unit.Position + Vector3.Normalize(unit.Position - me.Position) * 200, config.Item("packets").GetValue<bool>());
        }

        private static void Clear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(400)).ToList();
            if (minions.Count() > 2)
                {
                if (Items.HasItem(3077) && Items.CanUseItem(3077))
                Items.UseItem(3077);
                if (Items.HasItem(3074) && Items.CanUseItem(3074))
                Items.UseItem(3074);
                }
        }

        private static void Ulti()
        {

            if (!R.IsReady() || PingCasted) return;
            
                foreach (var allyObj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsAlly && !i.IsMe && !i.IsDead && i.CountEnemysInRange(750) >= 1 && (i.Health * 100 / i.MaxHealth) <= config.Item("atpercent").GetValue<Slider>().Value))
                {
                    
                    if (config.Item("user").GetValue<bool>() && R.IsReady() && me.CountEnemysInRange((int)Q.Range) < 1 && !config.Item("ult"+allyObj.SkinName).GetValue<bool>())
                    {
                        R.Cast(allyObj);
                        return;
                    }
                    else
                    {
                        Game.PrintChat("<font color='#ff0000'> Use Ultimate (R) to help: {0}</font>", allyObj.ChampionName);
                        for (Int32 i = 0; i < 2; i++)
                        {
                            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(allyObj.Position.X, allyObj.Position.Y, allyObj.NetworkId, 0, Packet.PingType.Fallback)).Process();
                        }
                    }
                    PingCasted = true;
                    Utility.DelayAction.Add(5000, () => PingCasted = false);
                }
            
        }


        private static void AutoQ()
        {
            if (!Q.IsReady() || me.HasBuff("Recall")) return;
            if (config.Item("autoqwithe").GetValue<bool>() || config.Item("autoqwithe").GetValue<bool>() && (currEnergy - me.Spellbook.GetManaCost(SpellSlot.Q) < eEnergy)) return;
            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target.Distance(me) < Q.Range)
            {
                if (target.IsValidTarget(Q.Range))

                    Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    currEnergy -= me.Spellbook.GetManaCost(SpellSlot.Q);
            }

        }
        private static void LasthitQ()
        {
            if (config.Item("autoqwithe").GetValue<bool>() && !(currEnergy - me.Spellbook.GetManaCost(SpellSlot.Q) > eEnergy)) return;
            var allMinions = MinionManager.GetMinions(me.ServerPosition, Q.Range);
            if (config.Item("autoqls").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                    HealthPrediction.GetHealthPrediction(minion,
                    (int)(me.Distance(minion) * 1000 / 1400)) <
                    me.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.CastOnUnit(minion);
                        currEnergy -= me.Spellbook.GetManaCost(SpellSlot.Q);
                        return;
                    }
                }
            }
        }
        private static void Harass(){
            if (config.Item("harassqwithe").GetValue<bool>() && !(currEnergy - me.Spellbook.GetManaCost(SpellSlot.Q) > eEnergy)) return;
            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (Q.IsReady() && config.Item("harassq").GetValue<bool>())
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                currEnergy -= me.Spellbook.GetManaCost(SpellSlot.Q);
            }

        }
        private static void Combo()
        {
            var minHit = config.Item("useemin").GetValue<Slider>().Value;

            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (W.IsReady() && config.Item("usew").GetValue<bool>() && currEnergy - me.Spellbook.GetManaCost(SpellSlot.W) >= eEnergy)
            {
            W.Cast();
            currEnergy -= me.Spellbook.GetManaCost(SpellSlot.W);
            }
            if (config.Item("usee").GetValue<bool>() && E.IsReady() && E.InRange(target.Position))
                {
                    if (minHit>1)
                    {
                        E.Cast(target.Position + Vector3.Normalize(target.Position - me.Position) * ((CheckingCollision(me, target, E, false, true).Count >= minHit) ? E.Range : 200), config.Item("packets").GetValue<bool>());
                    }
                    else E.Cast(target.Position + Vector3.Normalize(target.Position - me.Position) * 200, config.Item("packets").GetValue<bool>());
                }
            if (Q.IsReady() && config.Item("useq").GetValue<bool>() && currEnergy - me.Spellbook.GetManaCost(SpellSlot.E) >= eEnergy)
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                currEnergy -= me.Spellbook.GetManaCost(SpellSlot.Q);
            }
            UseItems(target);
 
        }
        private static float ComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;
            if (Q.IsReady() && me.Spellbook.GetManaCost(SpellSlot.Q)<me.Mana)
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.Q);
            if (E.IsReady() && me.Spellbook.GetManaCost(SpellSlot.E)<me.Mana)
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.E);
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Tiamat);
            }
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Hydra);
            }
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Botrk);
            }
            if (Items.HasItem(3146) && Items.CanUseItem(3146))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Hexgun);
            }
            if (Items.HasItem(3091) && Items.CanUseItem(3091))
            {
                damage += (float)me.CalcDamage(hero, Damage.DamageType.Magical, 42);
            }
            if (haspassive)
            {
                var passive = me.MaxHealth / 10 + 4+me.Level * 4;
                damage += (float)me.CalcDamage(hero, Damage.DamageType.Magical, passive);
            }
            if (me.SummonerSpellbook.CanUseSpell(me.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health - damage < (float)me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite))
            {
                damage += (float)me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            return damage;
        }

        private static void UseItems(Obj_AI_Hero target)
        {
            if (me.Distance(target) < 400)
            {
                if (Items.HasItem(3077) && Items.CanUseItem(3077))
                    Items.UseItem(3077);
                if (Items.HasItem(3074) && Items.CanUseItem(3074))
                    Items.UseItem(3074);
            }
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                bilgewater.Cast(target);
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153) && me.Health<me.MaxHealth/2)
            {
                botrk.Cast(target);
            }
            if (Items.HasItem(3146) && Items.CanUseItem(3146))
            {
                hexgun.Cast(target);
            }
            if (me.SummonerSpellbook.CanUseSpell(me.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                if (me.Distance(target) < 650 && ComboDamage(target) >= target.Health && (float)me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) >= target.Health)
                {
                    me.SummonerSpellbook.CastSpell(me.GetSpellSlot("SummonerDot"), target);
                }


            }
        }

        private static void DrawCircle(string menuItem, float spellRange)
        {
            Circle circle = config.Item(menuItem).GetValue<Circle>();
            if (circle.Active) Utility.DrawCircle(me.Position, spellRange, circle.Color);
        }

        public static List<Obj_AI_Base> CheckingCollision(Obj_AI_Base From, Obj_AI_Base Target, Spell Skill, bool Mid = true, bool OnlyHero = false)
        {
            var ListCol = new List<Obj_AI_Base>();
            foreach (var Obj in ObjectManager.Get<Obj_AI_Base>().Where(i => IsValid(i, Skill.Range) && Skill.GetPrediction(i).Hitchance >= HitChance.Medium && ((!OnlyHero && !(i is Obj_AI_Turret)) || (OnlyHero && i is Obj_AI_Hero)) && i != Target))
            {
                var Segment = (Mid ? Obj : Target).Position.To2D().ProjectOn(From.Position.To2D(), (Mid ? Target : Obj).Position.To2D());
                if (Segment.IsOnSegment && Obj.Position.Distance(new Vector3(Segment.SegmentPoint.X, Obj.Position.Y, Segment.SegmentPoint.Y)) <= Skill.Width + Obj.BoundingRadius) ListCol.Add(Obj);
            }
            return ListCol.Distinct().ToList();
        }

        public static bool IsValid(Obj_AI_Base Target, float Range = float.MaxValue, bool EnemyOnly = true, Vector3 From = default(Vector3))
        {
            if (Target == null || !Target.IsValid || Target.IsDead || !Target.IsVisible || (EnemyOnly && !Target.IsTargetable) || (EnemyOnly && Target.IsInvulnerable) || Target.IsMe) return false;
            if (EnemyOnly ? Target.IsAlly : Target.IsEnemy) return false;
            if ((From != default(Vector3) ? From : me.Position).Distance(Target.Position) > Range) return false;
            return true;
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (config.Item("autowwithe").GetValue<bool>() && !(currEnergy - me.Spellbook.GetManaCost(SpellSlot.Q) > eEnergy)) return;
            if (sender is Obj_SpellMissile && sender.IsValid && config.Item("autow").GetValue<bool>() && W.IsReady())
            {
                var missle = (Obj_SpellMissile)sender;
                var caster = missle.SpellCaster;
                if (caster.IsEnemy)
                {
                    var ShieldBuff = new Int32[] { 60, 100, 140, 180, 200 }[W.Level - 1] + 0.6 * me.FlatMagicDamageMod;
                    if (missle.SData.Name.Contains("BasicAttack"))
                    {
                        if (missle.Target.IsMe && ShieldBuff / 100 * config.Item("wabove").GetValue<Slider>().Value < caster.GetAutoAttackDamage(me, true))
                        {
                            W.Cast();
                            currEnergy -= me.Spellbook.GetManaCost(SpellSlot.W);
                        }
                    }
                    else if (missle.Target.IsMe || missle.EndPosition.Distance(me.Position) <= 130)
                    {
                        if (missle.SData.Name == "summonerdot")
                        {
                            if (me.Health <= (caster as Obj_AI_Hero).GetSummonerSpellDamage(me, Damage.SummonerSpell.Ignite) && ShieldBuff < (caster as Obj_AI_Hero).GetSummonerSpellDamage(me, Damage.SummonerSpell.Ignite))
                            {
                                W.Cast();
                                currEnergy -= me.Spellbook.GetManaCost(SpellSlot.W);
                            }
                        }
                        else if (ShieldBuff / 100 * config.Item("wabove").GetValue<Slider>().Value < (caster as Obj_AI_Hero).GetSpellDamage(me, (caster as Obj_AI_Hero).GetSpellSlot(missle.SData.Name, false), 1))
                        {
                            W.Cast();
                            currEnergy -= me.Spellbook.GetManaCost(SpellSlot.W);
                        }
                    }
                }
            }
        }
    }
}
