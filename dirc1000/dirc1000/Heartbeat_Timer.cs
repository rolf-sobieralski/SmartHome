using System.Windows.Forms;

//Hearbeat Timer ist ein leicht angepasster Timer um einfacher die gewünschten funktionen bei den Verbundenen Clients aufrufen zu können,
//wie der name bereits vermuten lässt, ist dieser timer dazu da um einen Heartbeat vom Client zu resetten und den Client ggf. aus der liste zu entfernen
namespace dirc1000
{
    public class Heartbeat_Timer : System.Timers.Timer
    {
        public string Client_IP{ get; set; }
        public Button client_button { get; set; }
        public int lap_count { get; set; }

        public Heartbeat_Timer() : base()
        {
        }
    }
}
