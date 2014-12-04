#region
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
#endregion

namespace Alistar
{
    internal class Program
    {
    	private static Menu Config;
        private static Obj_AI_Hero Player;
		private static Spell Q;
		private static Spell W;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
           	if (Player.ChampionName != "Alistar") return;
           	
           	Q = new Spell(SpellSlot.Q, 365);                                       
            W = new Spell(SpellSlot.W, 650);
                          							
			Config = new Menu("FA Alistar", "FA Alistar", true);
			
			var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
			SimpleTs.AddToMenu(targetSelectorMenu);
			Config.AddSubMenu(targetSelectorMenu);
			
            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            var orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
						
			Config.AddSubMenu(new Menu("Settings", "Settings"));
			Config.SubMenu("Settings").AddItem(new MenuItem("Combo", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
			Config.SubMenu("Settings").AddItem(new MenuItem("UseQ", "Use Q to Interrupt Spells").SetValue(true));
			Config.SubMenu("Settings").AddItem(new MenuItem("UseW", "Use W to Interrupt Spells").SetValue(true));
			Config.SubMenu("Settings").AddItem(new MenuItem("UsePacket", "Use Packet Cast").SetValue(true));
			           
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
			Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));	
			Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));			
			Config.AddToMainMenu();
            
            Game.PrintChat("FA Alistar loaded!");

			Game.OnGameUpdate += Game_OnGameUpdate;
			Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			Drawing.OnDraw += Drawing_OnDraw;
        }
        
        private static bool Packets()
        {
        	return Config.Item("UsePacket").GetValue<bool>();
        }
              
        private static void Game_OnGameUpdate(EventArgs args)
        {
			var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
			if (Config.Item("Combo").GetValue<KeyBind>().Active) Combo(target);
        }
        
        private static void QCast(Obj_AI_Base target)
        {
        	if (Player.Distance(target.ServerPosition) <= Q.Range && !W.IsReady()) Q.CastOnUnit(Player,Packets());
        }
        
        private static void Combo(Obj_AI_Base target)
        {
        	if (target == null) return;
        	if (Player.Distance(target.ServerPosition) <= Q.Range) 
        	{
        		if (Q.IsReady()) Q.CastOnUnit(Player,Packets());
        	}
        	else
        	{
        		if (Q.IsReady() && W.IsReady())
        		{
        			W.CastOnUnit(target,Packets());
        			var delay = Math.Max(0, Player.Distance(target) - 500)*10/25 + 25;
        			Utility.DelayAction.Add((int) delay, () => QCast(target));
        		}
        	}
        }
        
        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
        	if (Player.Distance(unit.ServerPosition) <= Q.Range && Q.IsReady()) Q.CastOnUnit(Player,Packets());
        	else if (Player.Distance(unit.ServerPosition) <= W.Range && W.IsReady()) W.CastOnUnit(unit,Packets());
        }
              
        private static void Drawing_OnDraw(EventArgs args)
        {
        	var drawQ = Config.Item("QRange").GetValue<Circle>();
            if (drawQ.Active && !Player.IsDead) Utility.DrawCircle(Player.Position, Q.Range, drawQ.Color);
	        var drawW = Config.Item("WRange").GetValue<Circle>();
            if (drawW.Active && !Player.IsDead) Utility.DrawCircle(Player.Position, W.Range, drawW.Color);
        }                    
    }
}