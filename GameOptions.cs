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
        public int TickSpeed = 20;                  // сколько тиков должно пройти, чтобы они обнулились и начали считаться заново
        public int TickPerFrame = 10;               // сколько мс длится 1 тик
        public string Version = "REV 11";           // текущая версия
        public string MapFilePath = "map.txt";      // путь до папки
        public string SpritesPath = "Assets";       // путь до спрайтов

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