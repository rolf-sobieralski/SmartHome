using System.Drawing;


//SmartHomeBereich kann man als Stockwerke oder Räume des Smart Home ansehen, diese Klasse hält grundlegende Infos zum jeweiligen bereich selbst bereit

namespace dirc1000
{
    class SmartHomeBereich
    {
        public Image Bild { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string bildName { get; set; }
        public SmartHomeBereich(Image bild,string bereichname, int id,string bildname)//Initialisierung des Smart Home bereichs
        {
            Bild = bild;//Bild zur späteren verarbeitung speichern
            ID = id;//ID zur späteren verarbeitung speichern
            Name = bereichname;//Name des Bereiches zur späteren verarbeitung speichern
            bildName = bildname;//Name des Bildes zur späteren verarbeitung speichern
        }
    }
}
