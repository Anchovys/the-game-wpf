using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptionsLoader;

namespace the_game_wpf
{
    public class GameOptions
    {
        public static readonly string NormalConfigFile = "conf.cfg";
        public int TickSpeed = 30;
        public int TickPerFrame = 10;

        public string MapFilePath = "map.txt";
        public string SpritesPath = "assets";

        //public GameOptions Clone()
        //{
        //    return (GameOptions)MemberwiseClone();
        //}

        public bool Push()
        {
            return Loader.Save(this, NormalConfigFile);
        }

        public string GetText()
        {
            return File.ReadAllText(NormalConfigFile);
        }

        public bool Pop(GameOptions options)
        {
            return Loader.Load(options, NormalConfigFile);
        }

    }
}