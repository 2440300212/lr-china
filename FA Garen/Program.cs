#region
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace GarenUlt
{
    internal class Program
    {
        private static Menu Config;
        private static Obj_AI_Hero myHero;
		private static Spell R;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            myHero = ObjectManager.Player;
            
           	if (myHero.ChampionName != "Garen") return;
           	
            R = new Spell(SpellSlot.R, 400);
         							
			Config = new Menu("FA Garen", "FA Garen", true);
			
            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            var orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
			
			Config.AddSubMenu(new Menu("Ultimate", "Ultimate"));
			Config.SubMenu("Ultimate").AddItem(new MenuItem("AutoUlt", "ON:Silder,Off:Killable").SetValue(new KeyBind("L".ToCharArray()[0],KeyBindType.Toggle,false)));
			Config.SubMenu("Ultimate").AddItem(new MenuItem("HP", "Auto R if %HP").SetValue(new Slider(10,100,0)));
			Config.AddToMainMenu();  
            
            Game.PrintChat("FA Garen loaded!");

			Game.OnGameUpdate += Game_OnGameUpdate;
        }
       
        private static void Game_OnGameUpdate(EventArgs args)
        {
		    var AutoUlt = Config.Item("AutoUlt").GetValue<bool>();
        	var HP = Config.Item("HP").GetValue<Slider>().Value;
        	foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget(450)))
        	{
        		if (Vector3.Distance(myHero.Position, enemy.Position) <= 400)
        		{
        			var TargetHP = enemy.Health/enemy.MaxHealth*100;
        			var DmgUlt = Damage.GetSpellDamage(myHero,enemy,SpellSlot.R);
        			if ( (TargetHP <= HP && AutoUlt) || (enemy.Health < DmgUlt && !AutoUlt) )
        				if (!enemy.HasBuff("JudicatorIntervention") || !enemy.HasBuff("Undying Rage")) R.Cast(enemy,true);        					
        		}      
        	}
        }
    }
}