using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Windows.Forms;

//Der SmartHomeClient ist das aufbauende Gerüst für die Beanhldung der verbundenen Clients

namespace dirc1000
{
    class SmartHomeClient
    {
        int id;
        TcpClient _Client;
        Button Client_button = null;
        bool alive;
        public int ebene { get; }
        Heartbeat_Timer client_timer;
        ObservableCollection<SmartHomeClient> clientlist;
        public string ClientName { get; set; }
        public SmartHomeClient(TcpClient client, int client_id)//Initialisieren des Client
        {
            _Client = client;//zuweisen des grundlegenden TCP Client
            id = client_id;//ID des Smart Home Client
            alive = true;//Active auf True setzen, damit Button und heartbeat korrekt arbeiten können
        }
        public Heartbeat_Timer Client_Timer//Heartbeat Timer um die Reale TCP Verbindung des TCP Client zu überwachen
        {
            get
            {
                return client_timer;
            }
            set
            {
                client_timer = value;
            }
        }
        public TcpClient _TcpClient//TCP Verbindung des Smart Home Client zurückgeben, sollte während der Laufzeit nicht geändert werden, daher auch nur Get und kein Set
        {
            get
            {
                return _Client;
            }
        }
        public void HeartBeat_Start(ObservableCollection<SmartHomeClient> Client_List)//Heartbeat Timer aufsetzen und starten
        {
            clientlist = Client_List;//aktuelle Liste der verbundenen Clients zur späteren Nutzung speichern
            Heartbeat_Timer alive_timer = new Heartbeat_Timer();//Timer initialisieren
            alive_timer.Interval = 5000;//Timer Interval setzen
            alive_timer.Client_IP = ((IPEndPoint)this._TcpClient.Client.RemoteEndPoint).Address.ToString();//IP an dieses Clients an den Timer übergeben
            alive_timer.Elapsed += new ElapsedEventHandler(alivecallback);//Event Funktion dem Timer zuweisen
            alive_timer.Start();//Timer starten
            client_timer = alive_timer;//Timer zur späteren Nutzung speichern
        }
        public void HeartBeat_Stop()//Timer anhalten, recht selbst erklärend
        {
            client_timer.Stop();
        }
        public void alivecallback(object sender, ElapsedEventArgs e)//Funktion für den Heartbeat Timer zum bearbeiten des Alive Status
        {
            if (alive)//check ob Client noch verfügbar ist(erkennbar daran, das der Client, so lange er Verbunden ist alle 2 Sekunden die Zahl 2 an den Listener sendet)
            {
                alive = false;//alive auf false setzen um Variable beim nächsten durchgang wieder zu prüfen
            }
            else
            {
                if (Client_Button != null)//check ob Client einen Button hat
                {
                    Client_button.ForeColor = Color.Red;//Text Farbe des zugehörigen Button auf Rot setzen
                    Client_button = null;//Button entfernen
                }
                    ((Heartbeat_Timer)sender).Close();//Timer schließen
                clientlist.Remove(this);//Client aus der Liste entfernen

            }
        }
        public void Disconnect()//Verbindung zum Client trennen, z.b. beim Shutdown des Servers
        {
            client_timer.Stop();//Timer anhalten
            client_timer.Close();//Timer schließen
            alive = false;//Client ist nun nichtmehr verfügbar
            if (Client_Button != null)//Check ob Client Button vorhanden ist
                Client_button.ForeColor = Color.Red;//Textfarbe des Button auf Rot ändern
            Client_button = null;//Button entfernen
            _Client.Close();//TCP Client Verbindung trennen
        }
        public bool HatButton()//Funktion zum prüfen ob der Client bereits einen zugewiesenen Button hat
        {
            if (Client_button == null)//check ob Button nicht vorhanden ist
            {
                return false;//false zurückgeben
            }
            else
            {
                return true;//true zurückgeben
            }
        }
        public Button Client_Button//Funktion um den zugewiesenen Button selbst zurückgeben oder festzulegen (da Button mit null initialisiert wird, funktion hier ausgeschrieben) 
        {
            get
            {
                return Client_button;//Button zurückgeben
            }
            set
            {
                Client_button = value;// Button zuweisen
            }

        }
        public void SetAlive(bool value)//diese Funktion wird vom Server aufgerufen, wenn der Client die Zahl 2 gesendet hat
        {
            alive = value;//Wert setzen
        }
        public int GetId()//hiermit erhält man die ID des Smart Home Clients, aktuell noch ohne Nutzen
        {
            return id;//ID zurückgeben
        }
        public bool IsAlive()//Rückgabe funktion ob der Client Verfügbar ist oder nicht
        {
            return alive;//Wert zurückgeben
        }
    }
}
