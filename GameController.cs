using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace the_game_wpf
{
    class GameController
    {
        public Map mainMap;
        public HeroObject heroObject;

        public GameController()
        {
            mainMap = new Map("map.txt");
            heroObject = (HeroObject)mainMap.FindObject(new HeroObject());
        }


    }
}
