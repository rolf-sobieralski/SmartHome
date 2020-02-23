using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;
using System.IO;
using NICE_LOGGER;

//Haupt Form für 
//Direct
//Inhome
//Remote
//Control
//Kurz DIRC. in dieser Form ist die Haupt Funktionalität des Servers enthalten


/*TODO's
 * Funktionen um erste Initialisierung von neu Verbunden Clients mittels übertragener Commands zu realisieren
 * Funktionen um bei Client Reconnects den von Client übermittelten Status der einzelnen Commands zu verarbeiten
 * Schnittstelle für Spracherkennungs Software erstellen
 * Schnittstelle für Smartphone App erstellen
 * Software Client erstellen
 */

namespace dirc1000
{
    public partial class Form1 : Form
    {

        TcpListener tcp_server;
        private ObservableCollection<SmartHomeClient> clientlist;
        Thread conn_listener;
        UdpClient server_anouncer;
        string di_br_adresse;
        string discovery_message;
        System.Timers.Timer udp_discovery;
        public bool server_running = false;
        string eintrag;
        Button akt_button = null;
        SMARTHOMEDB DB;
        List<Button> Button_List;
        string akt_client_ip;
        DataGridViewRow akt_command_reihe;
        string OldName;
        string OldValue;
        SmartHomeBereich akt_ebene = null;
        List<SmartHomeBereich> SmartHomeBereiche;
        WebGUI Web;
        RemoteControl RC;
        NiceLogger NL;
        bool udp_anouncer_active;

        public Form1()
        {
            InitializeComponent();
        }
        public object get_clientlist()//Funktion zum abrufen der aktuell Verbundenen Clients
        {
            ObservableCollection<SmartHomeClient> akt_list = clientlist;
            return akt_list;
        }
        private void button1_Click(object sender, EventArgs e)//Funktion zum Starten des Servers
        {
            try
            {
                clientlist = new ObservableCollection<SmartHomeClient>();
                clientlist.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnClientListChanged);
                if (!server_running)
                {
                    StringBuilder ip_daten = new StringBuilder(); // einfacher String um eine Liste von IP Adressen aufzunehemn
                    bool erste_adresse = true;
                    foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) //Loop durch alle Netzwerkkarten
                    {
                        if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)//check ob gefundene Karte WLAN oder LAN ist
                        {
                            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses) //Loop durch IP Infos
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork && erste_adresse) // check ob IP eine Externe oder Interne ist
                                {                                                                            //ToDo: für mehrere IP's einen string mit trenner erstellen und auf den Clients
                                                                                                             //zu jeder Adresse einen Connect versuch starten
                                    byte[] server_anouncement = Encoding.ASCII.GetBytes(ip.Address.ToString());     //IP Adresse als Byte Array bekommen
                                    ip_daten.AppendFormat("{0}", Encoding.ASCII.GetString(server_anouncement, 0, server_anouncement.Length)); // IP wird an string angehängt
                                    erste_adresse = false;                                                          //schmutzige Variante um nur eine IP zu bekommen, hier könnte noch ein Else zum If rein um den string für alle Adressen auszuweiten
                                }
                            }
                        }
                    }
                    discovery_message = ip_daten.ToString();//IP String neu zuweisen damit nicht mit dem orginalen gearbeitet werden muss
                    string[] ip_split = discovery_message.Split('.');//String trennen, um die Adresse für einen UDP Directed Unicast zu bekommen, sollte bei den meisten Heimnetzwerken funktionieren
                    di_br_adresse = ip_split[0] + "." + ip_split[1] + "." + ip_split[2] + ".255";//IP Directed Broadcast Adresse
                    tcp_server = new TcpListener(IPAddress.Any, 31337);//Server auf angegebenem Port(31337) wird erstellt.
                    conn_listener = new Thread(new ParameterizedThreadStart(onClientAdd));//Server in separatem Thread starten um Clients separat zu verarbeiten
                    server_anouncer = new UdpClient();//Initialisierung des UDP Directed Broadcast
                    udp_anouncer_active = true;
                    server_anouncer.EnableBroadcast = true;
                    server_anouncer.Client.Bind(new IPEndPoint(IPAddress.Any, 1337));
                    udp_discovery = new System.Timers.Timer(5000);//Timer für den UDP Directed Broadcast erstellen, Elapsed Event Handler zuweisen und Timer starten
                    udp_discovery.Elapsed += new ElapsedEventHandler(udp_server_discovery);
                    udp_discovery.Start();
                    conn_listener.Start(listBox1);//Threadstart den eigendlichen Server
                    server_running = true;
                }
                else
                {
                    MessageBox.Show("Server läuft bereits");
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim Starten des Servers (" + ex.ToString() + ")");
            }

        }
        void OnClientListChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)//wird aufgerufen, wenn Client der Liste hinzufefügt oder entfernt wird
        {
            this.Invoke((MethodInvoker)(() => listBox1.Items.Clear()));//Listbox leeren damit anschließend neu aufgebaut werden kann
            foreach (SmartHomeClient sc in clientlist) // Loop durch Clients, hier ist bereits der neue Client vorhanden, bzw der alte bereits entfernt.
            {
                this.Invoke((MethodInvoker)(() => listBox1.Items.Add(((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString())));//hier wird jeder Client aus der Clientlist als IP Adresse hinzugefügt.
            }
        }
        public void onClientAdd(object lf)//Server start und Client Handling
        {
            tcp_server.Start();
            bool already_in_list = false;//zum prüfen ob Client bereits in der Liste vorhanden ist(bei Verbindungsproblemen oder nicht genehmigtem versuch einen Client zu duplizieren)
            while (server_running)
            {
                try
                {
                    
                    //if (tcp_server.Pending())//prüfung ob eine Verbindung vorhnaden ist, die nochnicht akzeptiert wurde
                    //{
                        SmartHomeClient client = new SmartHomeClient(tcp_server.AcceptTcpClient(), clientlist.Count + 1);//neuer Smart Home Client wird erstellt
                        foreach (SmartHomeClient sc in clientlist)//Loop durch die Clientlist um duplikat zu prüfen
                        {
                            if (((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString() == ((IPEndPoint)client._TcpClient.Client.RemoteEndPoint).Address.ToString())
                            {
                                already_in_list = true;
                            }
                        }

                        if (!already_in_list)//wenn client nicht vorhanden ist einen neuen Thread für den Client starten
                        {
                        if (DB.client_exists(((IPEndPoint)client._TcpClient.Client.RemoteEndPoint).Address.ToString()))//check ob Client bereits in der Datewnbank vorhanden ist
                        {
                            List<string> client_infos = DB.fill_client_data(((IPEndPoint)client._TcpClient.Client.RemoteEndPoint).Address.ToString());//Datenbank Abfrage zum ausgewählten Smart Home Client
                            client.ClientName = client_infos[2];//ClientName aus der Datenbank dem aktuellen Client zuweisen
                        }
                            Thread client_thread = new Thread(new ParameterizedThreadStart(connected_client));
                            client_thread.Start(client);
                        }
                        else//wenn Client bereits vorhanden ist, verbindung schließen und bool zur sicherheit wieder auf false setzen
                        {
                            client._TcpClient.Close();
                            already_in_list = false;
                        }
                    //}
                }

                catch(InvalidOperationException ex) //leider gibt es beim TCP Listener einige ausnahmen die auftreten können, auch wen nder Server korrekt läuft bzw korrekt geschlossen wurde
                {
                    if (ex.HResult.ToString() != "-2146232798" && ex.HResult.ToString() != "-2146233079")
                    {
                        WriteLog(LogType.Error, "InvalidOperationException " + ex.HResult.ToString() + " beim hinzufügen eines neuen Clients (" + ex.ToString() + ")");
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode != 10004)
                    {
                        WriteLog(LogType.Error, "SocketException " + ex.ErrorCode + "beim hinzufügen eines neuen Clients (" + ex.ToString() + ")");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.HResult.ToString() != "-2146232798")
                    {
                        WriteLog(LogType.Error, "Exception " + ex.HResult.ToString() + "beim hinzufügen eines neuen Clients (" + ex.ToString() + ")");
                    }
                }
            }

        }
        public void udp_server_discovery(object sender, ElapsedEventArgs e)// UPD Directed Broadcast. hier wird alle 5 sekunden ein Paket an alle Adressen im Subnet gesendet
        {
            try
            {
                server_anouncer.Send(Encoding.ASCII.GetBytes(discovery_message), discovery_message.Length, di_br_adresse, 1337);
                WriteLog(LogType.Info, "UDP Discover gesendet an 192.168.0.255");
            }
            catch (Exception ex)
            {
                if(ex.HResult != -2146232798)
                {
                    WriteLog(LogType.Error, "Ausnahme im UDP Discovery aufgetreten\n" + ex.Message.ToString() + "\nNummer: " + ex.HResult.ToString());
                }
            }
        }
        void WriteLog(LogType LType, string message)//funktion für den selbst geschriebenen Logger, um Log dateien im Programm Startverzeichniss abzulegen.
        {
            NL.WriteToLog(LType, message);
        }
        public void connected_client(object client)//Smart Home Client Handling
        {
            try
            {
                SmartHomeClient CL = (SmartHomeClient)client;//übergebenes Object zu smartHomeClient cast
                bool cl_connected = true;
                link_button(CL);//falls Client IP bereits in der Datenbank vorhanden ist, wird der Button zum Client wieder verbunden

                clientlist.Add(CL);//Client wird der ClientList hinzugefügt
                CL.HeartBeat_Start(clientlist);// Sobald Client Verbunden ist, sollte er alle 2 Sekunden ein Lebenszeichen von sich geben, damit der Server weis, das der Client noch erreichbar ist
                while (cl_connected)
                {
                    if (!CL._TcpClient.GetStream().CanRead)// check ob aus dem ClientStream gelesen werden kann(einfache prüfung ob der Client noch verbunden ist)
                    {
                        CL.Disconnect();// Client wird getrennt
                        cl_connected = false;
                    }
                    else
                    {
                        //if (CL._TcpClient.GetStream().DataAvailable)//Check ob daten im Stream vorhanden sind z.b. der Heartbeat
                        //{
                            byte[] readBuffer = new byte[1024];//Buffer für die Daten erstellen, sollte reichen, insbesondere, da der Client aktuell nur den Heartbeat sendet
                            int anzahlBytes = 0;
                            StringBuilder client_recv_string = new StringBuilder();
                            do//String zur weiteren verarbeitung der Daten des Clients erstellen
                            {
                                anzahlBytes = CL._TcpClient.GetStream().Read(readBuffer, 0, readBuffer.Length);
                                client_recv_string.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, anzahlBytes));
                            } while (CL._TcpClient.GetStream().DataAvailable);

                            if (client_recv_string.ToString() == "2")//Check ob Daten Heartbeat pakete sind
                            {
                                CL.SetAlive(true);//bool im Smart Home Client setzen, um disconnect von Server zu verhindern
                                WriteLog(LogType.Info, ((IPEndPoint)CL._TcpClient.Client.RemoteEndPoint).Address.ToString() + "- " + client_recv_string.ToString());
                            }
                            else
                            {
                                WriteLog(LogType.Warning, ((IPEndPoint)CL._TcpClient.Client.RemoteEndPoint).Address.ToString() + "- Funktion nicht implementiert");
                            }
                        //}
                    }

                }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, ex.ToString());
            }
        }
        public void remove_clients()//alle Clients aus der Client list werfen(z.b. bei Server Stop)
        {
            try
            {
                if (clientlist != null)
                {
                    for (int i = 0; i < clientlist.Count; i++)
                    {
                        SmartHomeClient client = clientlist[0];
                        client.Disconnect();
                        i++;
                    }
                    clientlist.Clear();
                }


                this.Invoke((MethodInvoker)(() => listBox1.Items.Clear()));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim leeren der ClientList(" + ex.ToString() + ")");
            }
        }
        private void button2_Click(object sender, EventArgs e)//Server Stoppen, Clientlist leeren, Listboxen leeren
        {
            try
            {
                if (clientlist != null)
                    clientlist.CollectionChanged -= OnClientListChanged;//handler entfernen, damit beim geregelten Shutdown nichtmehr nach Changes geprüft wird.

                tcp_server.Stop();
                remove_clients();
                server_anouncer.Close();
                server_running = false;
                udp_discovery.Stop();
                clientlist = null;
                DataTable client_table = (DataTable)akt_smart_client.DataSource;
                DataTable command_table = (DataTable)CommandList.DataSource;
                if (client_table != null)
                {
                    client_table.Clear();//Daten von evtl ausgewähltem Client entfernen
                }
                if (command_table != null)
                {
                    command_table.Clear();//Client command List daten entfernen
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim stoppen des Servers(" + ex.ToString() + ")");
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)//ungeregelter Shutdown, sauber machen was noch möglich ist.
        {
            try
            {
                if (server_running)
                {
                    tcp_server.Stop();
                    server_anouncer.Close();
                    server_running = false;
                    doDummyClientConnect();
                    clientlist = null;
                }
                    udp_anouncer_active = false;
                    udp_discovery.AutoReset = false;
                Web.Stop();
                RC.StopRemoteControl();
                NL.CloseLogFile();
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim schließen des Servers (" + ex.ToString() + ")");
                NL.CloseLogFile();
            }
        }
        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)//funktion zum ggf. erstellen eines Button für den Smart Home client
        {
            try
            {
                if (akt_ebene != null && akt_ebene.ID != 0)//check ob überhaupt eine Ebene Vorhanden ist
                {
                    if (listBox1.Items.Count == 0)// Check ob irgend ein Client verbunden ist
                    {
                        return;
                    }
                    int index = listBox1.IndexFromPoint(e.Location);//Index anhand des Maus Cursors ermitteln
                    eintrag = listBox1.Items[index].ToString();
                    foreach (SmartHomeClient sc in clientlist)//Loop durch die ClientList
                    {
                        if (((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString() == eintrag && !sc.HatButton())//Hat dieser Client bereits einen Button?
                        {
                            Point maus_position = new Point(e.X + 5, e.Y + 5);//Initial Position für den Button
                            Button smartHomeButton = new Button();
                            smartHomeButton.AccessibleName = eintrag;
                            smartHomeButton.Width = 20;
                            smartHomeButton.Height = 20;
                            smartHomeButton.Text = "S";
                            smartHomeButton.Click += new EventHandler(OnSmartHomeButtonClick);//OnClick Event hinzufügen
                            smartHomeButton.KeyDown += new KeyEventHandler(OnSmartHomeButtonDown);//OnKEyDown Event hinzufügen, mit der Entf Taste kann der button wieder gelöscht werden
                            smartHomeButton.Location = maus_position;//Button auf der erstellten Position generieren
                            smartHomeButton.BringToFront();//Button nach vorn holen
                            placeableImageBox1.Controls.Add(smartHomeButton);//Button zur ImageBox hinzufügen
                            akt_button = smartHomeButton;//hier wird der gerade erstellte Button zum akt_button gemacht um unabhängig darauf zugreifen zu können
                            sc.Client_Button = smartHomeButton;//Button an Smart Home Client zuweisen
                        }
                        else
                        {
                            if (((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString() == eintrag && sc.HatButton())//wenn Button bereits in der Datenbank ist wird der Button mit einem Timer zum Blinken gebracht
                            {
                                Heartbeat_Timer t = new Heartbeat_Timer();
                                t.Interval = 500;
                                t.client_button = sc.Client_Button;
                                t.lap_count = 0;
                                t.Elapsed += new ElapsedEventHandler(blink_button);
                                t.Start();

                            }
                        }
                    }
                }
                else//Hinweis zeigen das eine Ebene erstellt werden muss
                {
                    MessageBox.Show("keine Ebene vorhanden. füge eine Smart Home Ebene hinzu", "ACHTUNG!");
                    but_neue_ebene.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim Einfügen eines neuen SMART Home Clients (" + ex.ToString() + ")");
            }
        }
        void blink_button(object sender, ElapsedEventArgs e)// Timer Funktion um einen Smart Home Button zum blinken zu bringen
        {

            try
            {
                if (((Heartbeat_Timer)sender).client_button.BackColor == Color.Yellow)
                {
                    ((Heartbeat_Timer)sender).client_button.BackColor = Color.Transparent;
                }
                else
                {
                    ((Heartbeat_Timer)sender).client_button.BackColor = Color.Yellow;
                }
            ((Heartbeat_Timer)sender).lap_count++;
                if (((Heartbeat_Timer)sender).lap_count >= 9)
                {
                    ((Heartbeat_Timer)sender).client_button.BackColor = Color.Transparent;
                    ((Heartbeat_Timer)sender).Stop();
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler bei Button HINT (" + ex.ToString() + ")");
            }
        }
        void OnSmartHomeButtonClick(object sender, EventArgs e)//Übergabe funktion für Smart Client Infos
        {
            get_SmartHomeClientInfos(((Button)sender).AccessibleName);
        }
        void get_SmartHomeClientInfos(string client_ip)//Funktion um Infos vom Smart Home Client anhand der Übergebenen IP zu holen
        {
            try
            {
                List<string> client_infos = DB.fill_client_data(client_ip);//Datenbank Abfrage zum ausgewählten Smart Home Client
                List<List<string>> client_commands = DB.fill_client_commands(client_infos[1]);//das gleiche normal, diesmal für die hinzugefügten Commands des Clients
                DataTable client_table = new DataTable();//DataTable für Client Infos
                DataColumn spalte_1 = new DataColumn("Beschreibung");//erste Spalte für Beschreibung
                spalte_1.DataType = System.Type.GetType("System.String");//Typ der Spalte
                spalte_1.ReadOnly = true;//diese Spalte darf nacht bearbeitet werden
                spalte_1.Unique = true;//diese Spalte ist immer eindeutig
                spalte_1.Caption = "Beschreibung";//Text der als Überschrift angezeigt wird
                client_table.Columns.Add(spalte_1);//erste Spalte hinzufügen
                DataColumn spalte_2 = new DataColumn("wert");//das gleiche nochmal für die Werte
                spalte_2.DataType = System.Type.GetType("System.String");
                spalte_2.ReadOnly = false;//diese Spalte kann bearbeitet werden
                spalte_2.Unique = false;//und muss auch nicht einzigartig sein
                spalte_2.Caption = "Wert";//Überschrift der zweiten spalte
                akt_client_ip = client_infos[1];//aktuelle Client IP aus der Abfrage holen und zuweisen
                client_table.Columns.Add(spalte_2);//Spalte zwei (Werte) hinzufügen
                var Reihe_1 = client_table.NewRow();//da wir immer nur ID, Name und IP haben
                var Reihe_2 = client_table.NewRow();//werden auch immer nur
                var Reihe_3 = client_table.NewRow();//drei Reihen hinzugefügt und befüllt
                Reihe_1["Beschreibung"] = "ID";//Beschreibungen für diese Reihen
                Reihe_2["Beschreibung"] = "IP";
                Reihe_3["Beschreibung"] = "Client Name";
                Reihe_1["wert"] = client_infos[0];//Werte für diese Reihen
                Reihe_2["wert"] = client_infos[1];
                Reihe_3["wert"] = client_infos[2];
                client_table.Rows.Add(Reihe_1);//Reihen werden hinzugefügt
                client_table.Rows.Add(Reihe_2);
                client_table.Rows.Add(Reihe_3);
                akt_smart_client.DataSource = client_table;//DataTable zuweisen
                Smart_Client_Infos.DataSource = akt_smart_client;// Daten an Tabelle binden
                Smart_Client_Infos.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                update_command_list();//Aufruf zur Verarbeitung der Client Commands
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim Holen der SMART Client Infos (" + ex.ToString() + ")");
            }
        }
        void OnSmartHomeButtonDown(object sender, KeyEventArgs e)//Check funktion zum Button löschen
        {
            if (e.KeyData == Keys.Delete)//prüfung of gedrückte Taste Entf ist
            {
                delete_SmartHomeButton((Button)sender);//Aufruf Funktion zum löschen des Buttons
            }
        }
        void delete_SmartHomeButton(Button but)//Funktion zum Löschen des Buttons
        {
            try
            {
                string IP = but.AccessibleName;//zugeförige IP Adresse des Buttons abrufen
                foreach (SmartHomeClient sc in clientlist)//Loop durch alle Smart Home Clients
                {
                    if (((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString() == IP)//Prüfung ob erhaltene IP mit der des Smart Home Client übereinstimmt
                    {
                        sc.Client_Button = null;//Button aus Smart Home Client entfernen
                        placeableImageBox1.Controls.Remove(but);//Button aus der ImageBox löschen
                        DB.remove_client_button(IP);//Button wird auch aus der Datenbank gelöscht
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim Löschen des SMART Home Client Buttons (" + ex.ToString() + ")");
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)//Funktion zum verarbeiten von Maus bewegungen auf der ImageBox
        {
            if (akt_button != null)//prüfung ob es einen Aktuellen Button gibt
            {
                akt_button.Location = new Point(e.X + 5, e.Y + 5);//Position des aktuellen Buttons an die Maus position anpassen
            }
        }
        private void button4_Click(object sender, EventArgs e)//Funktion zum erstellen neuer Commads
        {
            DataTable Command_Table = (DataTable)CommandList.DataSource;//Datatable erstellen
            var Neue_Reihe = Command_Table.NewRow();//neue Reihe für die Überschriften
            int count = Command_Table.Rows.Count;//Anzahl der Commands für den ausgewählten Client
            Neue_Reihe["Beschreibung"] = "Command_" + count;//Command Name 
            Neue_Reihe["Wert"] = 0;//initial Wert für Command
            Neue_Reihe["Status"] = 0;//Status(aktuell noch nicht genutzt) zum anzeigen ob Client die Anweisung verarbeitet hat
            CommandList.DataSource = Command_Table;//Datentable der CommandList zuweisen
            Command_Table.Rows.Add(Neue_Reihe);//erstellte Reihe dem Datatable hinzufügen
            Smart_Client_Commands.DataSource = CommandList;//CommandList dem SteuerElement hinzufügen
            DB.insert_command(akt_client_ip, "Command_" + count, "0");//gerade erstellten Command der Datenbank hinzufügen
        }
        void link_button(SmartHomeClient sc)//Funktion zum zuweisen eines Vorhandenen Button zu einem wieder verbundenen Client
        {
            foreach (Button but in Button_List)//Loop durch die vorhandenen Buttons
            {
                if (but.AccessibleName == ((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString())//Check ob der Button Name identisch mit der Client IP ist
                {
                    if (!sc.HatButton())//check das der Client noch keinen Button hat
                    {
                        sc.Client_Button = but;//Button zum Client zuweisen
                        but.ForeColor = Color.Black;//Textfarbe des Button ändern
                    }
                }
            }
        }
        private void placeableImageBox1_MouseDown(object sender, MouseEventArgs e)//Funktion zum Platzieren neuer Buttons
        {
            if (akt_button != null)//check ob akt_button vorhanden ist
            {
                akt_button.Location = e.Location;//button Position auf Maus Position setzen

                int id = 0;

                bool client_check = DB.client_exists(akt_button.AccessibleName);
                if (!client_check)//Check ob Client nicht vorhanden ist
                {
                    foreach (SmartHomeClient sc in clientlist)//Loop durch Smart Home Client Liste
                    {
                        id++;
                        string IP = ((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString();
                        if (akt_button.AccessibleName == IP)//Check ob IP identisc hmit dem Button namen ist
                        {
                            DB.insert_Client(id, IP, IP, akt_ebene.ID, e.X, e.Y);//Client in Datenbank eintragen
                        }
                    }
                    akt_button = null;
                }
                else
                {
                    DB.update_client(akt_button.AccessibleName, akt_ebene.ID, e.X, e.Y);//Wenn Client bereits vorhanden ist, wird nur die Position des Button in der Datenbank aktualisiert
                    akt_button = null;
                }
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)//aus und einblenden bereits vorhandener Clients
        {
            listBox1.Items.Clear();// Client übersicht leeren
            if (checkBox1.Checked == true)// check ob Checkbox markiert ist
            {
                foreach (SmartHomeClient sc in clientlist)//Loop durch ClientList
                {
                    if (sc.Client_Button == null)//cehck ob aktueller Client keinen Button hat
                    {
                        listBox1.Items.Add(((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString());//Client hinzufügen
                    }
                }
            }
            else//wenn checkbox nicht markiert ist alle Clients anzeigen
            {
                foreach (SmartHomeClient sc in clientlist)//loop durch die ClientList
                {
                    listBox1.Items.Add(((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString());//Client der Liste hinzufügen
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)//alles initialisieren und Datenbank prüfen
        {
            NL = new NiceLogger(richTextBox1, LogMode.Error, WindowType.RichTextBox);//Log Funktion initialisieren
            Smart_Client_Infos.DataSource = akt_smart_client;
            Smart_Client_Commands.DataSource = CommandList;
            DataGridViewButtonColumn button = new DataGridViewButtonColumn();//Button Spalte für die Commands erstellen
            Smart_Client_Commands.Columns.Add(button);//und Spalte hinzufügen
            Button_List = new List<Button>();//Button Liste erstellen
            DB = new SMARTHOMEDB(NL);//Datenbank Klasse initialisieren
            if (DB.CreateDatabase())//check ob Datenbank vorhanden ist
            {
                DB.OpenConnection(Application.StartupPath + "\\dircdb.sqlite");//Datenbank öffnen
                SmartHomeBereiche = DB.get_bereiche();//ebenen aus der Datenbank laden und zuweisen

                if (SmartHomeBereiche != null && SmartHomeBereiche.Count > 0)//Check ob Bereiche initialisiert sind und ob die Liste größer als 0 ist
                {
                    foreach (SmartHomeBereich sb in SmartHomeBereiche)//Loop durch die Bereiche
                    {
                        cb_bereiche.Items.Add(sb.Name);//Bereiche dem DropDown Menü hinzufügen
                    }
                    placeableImageBox1.BackgroundImage = SmartHomeBereiche[0].Bild;//Bild der ersten Ebene in der ImageBox laden
                    placeableImageBox1.BackgroundImageLayout = ImageLayout.Stretch;//Bild an Größe der ImageBox anpassen
                    akt_ebene = SmartHomeBereiche[0];//aktuelle ebene zuweisen, um später darauf zugreifen zu können
                    cb_bereiche.SelectedItem = cb_bereiche.Items[0];//ComboBox erstes vorhandenes Item auswählen
                }
                else
                {
                    akt_ebene = new SmartHomeBereich(this.BackgroundImage, null, 0, "noFile.png");//wenn keine Ebenen Vorhanden sind, eine temporäre ebene erstellen
                }

                List<List<string>> clients_in_db = DB.get_clients();//Clients aus Datenbank laden
                if (clients_in_db != null && clients_in_db.Count > 0)//check ob Clients geladen werden konnten und Sammlung größer als 0 ist
                {
                    foreach (List<string> smc in clients_in_db)//Loop durch die Clients
                    {
                        if (Convert.ToInt16(smc[4]) > 0 && Convert.ToInt16(smc[5]) > 0)//Check ob Koordinaten beide nicht 0 sind(passiert wenn der Button gelöscht wurde)
                        {
                            if (Convert.ToInt16(smc[3]) == akt_ebene.ID)//Check ob der Button auf der aktuell sichtbaren Ebene liegt
                            {
                                Button smc_but = new Button();
                                smc_but.AccessibleName = smc[1];//IP als Name des Button eintragen
                                smc_but.Text = "S";
                                if (!server_running)//check ob Server bereits läuft
                                {
                                    smc_but.ForeColor = Color.Red;// Button Textfarbe auf Rot setzen(hinweis das Client nicht verbunden ist)
                                }
                                smc_but.Width = 20;
                                smc_but.Height = 20;
                                smc_but.Location = new Point(Convert.ToInt16(smc[4]), Convert.ToInt16(smc[5]));//Position des Button setzen
                                smc_but.Click += new EventHandler(OnSmartHomeButtonClick);//Click Event Funktion hinzufügen
                                smc_but.KeyDown += new KeyEventHandler(OnSmartHomeButtonDown);//Funktion für Tastendruck hinzufügen
                                Button_List.Add(smc_but);//Button der Liste hinzufügen
                                placeableImageBox1.Controls.Add(smc_but);//Button der ImageBox hinzufügen
                            }
                        }
                    }
                }
            }
            Web = new WebGUI(this, NL, DB);//Web GUI initialisieren(aufrufbar über http://"Extern erreichbare IP Adresse" Port 8844 / oder http://localhost:8844
            RC = new RemoteControl();
            RC.StartRemoteControl(NL,this);


        }
        private void button5_Click(object sender, EventArgs e)//Funktion zum löschen des Ausgewählten Command zum Client
        {
            DB.delete_command(akt_client_ip, akt_command_reihe.Cells[1].FormattedValue.ToString());//Command aus Datenbank löschen
            update_command_list();//Command Liste aktualisieren
        }
        public void update_command_list()//Funktion zum aktualisieren der Command Liste zum aktuellen Client
        {
            List<List<string>> client_commands = DB.fill_client_commands(akt_client_ip);//Commands aus Datenbank holen
            DataTable command_table = new DataTable();
            DataColumn cSpalte_1 = new DataColumn("Beschreibung");
            cSpalte_1.DataType = System.Type.GetType("System.String");
            cSpalte_1.ReadOnly = false;//Command Name kann bearbeitet werden
            cSpalte_1.Unique = true;//muss aber einzigartig sein(für spätere nutzung in Verbindung mit Spracherkennungs Software wie Google Assistant, Amazon Alexa, Apple Siri oder anderer Open Source Software)
            cSpalte_1.Caption = "Beschreibung";
            command_table.Columns.Add(cSpalte_1);//spalte hinzufügen
            DataColumn cSpalte_2 = new DataColumn("Wert");
            cSpalte_2.DataType = System.Type.GetType("System.String");
            cSpalte_2.ReadOnly = false;//Wert der an den Client übermittelt wird kann bearbeitet werden
            cSpalte_2.Unique = false;//und muss auch nicht einzigartig sein(verschiedene Funktionen können ja das gleiche beim Client bewirken)
            cSpalte_2.Caption = "Wert";
            command_table.Columns.Add(cSpalte_2);//spalte Hinzufügen
            DataColumn cSpalte_3 = new DataColumn("Status");
            cSpalte_3.DataType = System.Type.GetType("System.String");
            cSpalte_3.ReadOnly = false;//testweise hier bearbeitung ermöglicht
            cSpalte_3.Unique = false;//und nicht einzigartig gesetzt, da hier in zukunft rückgabewerte angezeigt werden die der Client zur Funktion zurückgeben
            cSpalte_3.Caption = "Status";
            command_table.Columns.Add(cSpalte_3);//spalte Hinzufügen

            foreach (List<string> reihe in client_commands)//Loop durch Client Commands
            {
                var data_reihe = command_table.NewRow();//neue Reihe erstellen                          möglchkeit das Clients bereits mit vordefinierten Commands
                data_reihe["Beschreibung"] = reihe[0];//ergebnis in erste Spalte eintragen              erstellt werden können, die Sie dann selbst
                data_reihe["Wert"] = reihe[1];//ergebnis in zweite Spalte eintragen                     übermitteln und eingetragen werden
                data_reihe["Status"] = 0;//spalte 3 nur initialisieren                                  ist hier auch sinnvoll, wenn der Server z.b. abstürzt, die Clients aber bereits 
                command_table.Rows.Add(data_reihe);//Reihe hinzufügen                                   Funktionen ausgeführt haben
            }                                                                                           

            CommandList.DataSource = command_table;//Command Liste hinzufügen
            Smart_Client_Commands.DataSource = CommandList;//Daten zum Steuerelement zuweisen
            Smart_Client_Commands.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);//spalten größe automatisch anpassen
        }
        void update_client_infos()//Funktion zum Refresh der angezeigten Client Infos(ID, IP, Name)
        {
            List<string> client_infos = DB.fill_client_data(akt_client_ip);//Client daten holen
            DataTable client_table = new DataTable();                       //neuen Table erstellen
            DataColumn spalte_1 = new DataColumn("Beschreibung");           //neue Spalten
            DataColumn spalte_2 = new DataColumn("wert");                   //erstellen

            spalte_1.DataType = System.Type.GetType("System.String");       //Typ für Spalte
            spalte_2.DataType = System.Type.GetType("System.String");       //definieren

            spalte_1.ReadOnly = true;                                       //Readonly Flag
            spalte_2.ReadOnly = false;                                      //setzen
                                    
            spalte_1.Unique = true;                                         //Einzigartig flag
            spalte_2.Unique = false;                                        //setzen

            spalte_1.Caption = "Beschreibung";                              //Namen für Spalten
            spalte_2.Caption = "Wert";                                      //festlegen

            client_table.Columns.Add(spalte_1);                             //Spalten zum Table 
            client_table.Columns.Add(spalte_2);                             //hinzufügen

            akt_client_ip = client_infos[1];                                //akt_client_ip zuweisen um später darauf zugreifen zu können
            
            var Reihe_1 = client_table.NewRow();                            //drei Reihen zu den
            var Reihe_2 = client_table.NewRow();                            //Spalten
            var Reihe_3 = client_table.NewRow();                            //hinzufügen

            Reihe_1["Beschreibung"] = "ID";                                 //Text für erste Spalte
            Reihe_2["Beschreibung"] = "IP";                                 //der einzelnen
            Reihe_3["Beschreibung"] = "Client Name";                        //Reihen setzen

            Reihe_1["wert"] = client_infos[0];                              //
            Reihe_2["wert"] = client_infos[1];                              //Werte zu den einzelnen Reihen hinzufügen
            Reihe_3["wert"] = client_infos[2];                              //

            client_table.Rows.Add(Reihe_1);                                 //
            client_table.Rows.Add(Reihe_2);                                 //Reihen dem Table hinzufügen
            client_table.Rows.Add(Reihe_3);                                 //

            akt_smart_client.DataSource = client_table;                     //Table zuweisen
            Smart_Client_Infos.DataSource = akt_smart_client;               //Daten an Steuerelement binden
            Smart_Client_Infos.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells); //Spalten größe automatisch setzen
        }
        void onCommandButtonClick(string command_value)//Funktion zum Command senden an Verbundenen Client
        {

            foreach (SmartHomeClient sc in clientlist)//Loop durch die ClientList
            {
                if (((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString() == akt_client_ip)//check ob IP mit dem aktuellen Client übereinstimmt
                {
                    byte[] send_daten = new byte[command_value.Length];//Bytearray zum senden erstellen
                    Encoding.ASCII.GetBytes(command_value, 0, command_value.Length, send_daten, 0);// Command in Bytearray umwandeln
                    sc._TcpClient.GetStream().Write(send_daten, 0, command_value.Length);//Command in Stream schreiben
                    sc._TcpClient.GetStream().Flush();//stream Flush um senden der Daten sicher zu stellen
                }

            }
        }
        private void Smart_Client_Commands_SelectionChanged(object sender, EventArgs e)//Funktion zum zuweisen der aktuellen Command Reihe(wichtig um ggf. ausgewählten Command über Button zu löschen)
        {
            akt_command_reihe = Smart_Client_Commands.CurrentRow;

        }
        private void Smart_Client_Commands_CellEndEdit(object sender, DataGridViewCellEventArgs e)//Funktion um Commands zu aktuallisieren(Command Name , Command wert, Command Status)
        {
            DB.update_data(akt_client_ip, OldName, (string)Smart_Client_Commands[1, e.RowIndex].Value, OldValue, (string)Smart_Client_Commands[2, e.RowIndex].Value);//Datenbank aktualisieren
            update_command_list();// funktion zum aktuallisieren der angezeigten Commands
        }
        private void Smart_Client_Commands_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)//Funktion um den alten Namen und den alten Wert zu behalten damit beim Update in der Datenbank klar ist um welchen wert es sich handelt
        {
            OldName = (string)Smart_Client_Commands[1, e.RowIndex].Value;
            OldValue = (string)Smart_Client_Commands[2, e.RowIndex].Value;
        }
        private void Smart_Client_Commands_CellClick(object sender, DataGridViewCellEventArgs e)//Funktion um CommandButton zu klicken.(ist direkt mit dem Button in der Liste schwieriger zu bewerkstelligen)
        {
            if (e.ColumnIndex == 0)//check ob der Click in der ersten Spalte platziert ist
            {
                string command_value = Smart_Client_Commands.Rows[e.RowIndex].Cells[2].FormattedValue.ToString();//Command Wert zum senden ermitteln
                onCommandButtonClick(command_value + "1");//Aufruf zum senden des Command, die 1 am ende ist zum einfacheren verarbeiten auf der Client seite
            }
        }
        private void Smart_Client_Infos_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)//Funktion um alten Wert aus den Client Infos fest zu halten
        {
            OldName = (string)Smart_Client_Infos[1, e.RowIndex].Value;
        }
        private void Smart_Client_Infos_CellEndEdit(object sender, DataGridViewCellEventArgs e)//Funktion zum Client Infos aktualisieren
        {
            DB.update_data(akt_client_ip, (string)Smart_Client_Infos[1, e.RowIndex].Value);//Datenbank aktualisieren
            update_client_infos();//Aufruf Refresh der angezeigten Client Infos
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)//Funktion zum ändern der Client Infos die angezeigt werden, wenn ein neuer Client in der Liste ausgewählt wurde
        {
            string IP = listBox1.SelectedItem.ToString();//IP adresse bekommen nach der gesucht wird
            foreach (SmartHomeClient sc in clientlist)//Loop durch Client List
            {
                if (((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString() == IP && sc.HatButton())//Check ob aktueller Client mit der IP Adresse übereinstimmt und einen Button hat
                {
                    get_SmartHomeClientInfos(IP);//Infos zum aktuellen Client holen
                }
            }
        }
        private void bildLadenToolStripMenuItem_Click(object sender, EventArgs e)//Funktion zum zuweisen eines Hintergrund Bildes für eine Smart Home Ebene
        {
            if (OpenHGImage.ShowDialog() == DialogResult.OK)//Check ob Bild Auswahl bestätigt wurde
            {
                string FileOpenResult = OpenHGImage.FileName;
                string baseFileName = OpenHGImage.SafeFileName;
                if (akt_ebene.ID != 0)//check ob aktuelle Ebene anders als 0 ist
                {
                    if (!File.Exists(Application.StartupPath + "\\Bereiche\\" + baseFileName))//check das Datei mit gleichem Namen nochnicht im Programmverzeichnis vorhanden ist
                    {
                        File.Copy(FileOpenResult, Application.StartupPath + "\\Bereiche\\" + baseFileName);//Datei ins Programmverzeichnis kopieren
                        DB.update_Bereich(akt_ebene.ID, baseFileName);//Bereich in der Datenbank aktualisieren
                        Image HG_Bild = Image.FromFile(Application.StartupPath + "\\Bereiche\\" + baseFileName);//Hintergrundbild aus Datei erzeugen
                        placeableImageBox1.BackgroundImageLayout = ImageLayout.Stretch;//Bildgröße für Imagebox anpassen
                        placeableImageBox1.BackgroundImage = HG_Bild;//Hintergrundbild für ImageBox setzen
                    }
                    else
                    {
                        DB.update_Bereich(akt_ebene.ID, baseFileName);//Bereich in der Datenbank aktualisieren
                        Image HG_Bild = Image.FromFile(Application.StartupPath + "\\Bereiche\\" + baseFileName); //Hintergrundbild aus Datei erzeugen
                        placeableImageBox1.BackgroundImageLayout = ImageLayout.Stretch;//Bildgröße für Imagebox anpassen
                        placeableImageBox1.BackgroundImage = HG_Bild;//Hintergrundbild für ImageBox setzen
                    }
                }
                else
                {
                    MessageBox.Show("keine Ebene vorhanden. füge eine Smart Home Ebene hinzu", "ACHTUNG!");
                    but_neue_ebene.ForeColor = Color.Red;//Button Textfarbe in Rot ändern
                }
            }
        }
        private void contextMenuStrip1_VisibleChanged(object sender, EventArgs e)//Funktion zum Einfügen einer neuen Smart Home Ebene
        {
            if (Ebenen_Eingabe.Visible == false)//check ob ContexMenu gerade angezeigt wird oder nicht
            {
                if (toolStripTextBox1.Text.Trim() != "")//check ob irgendetwas in die Textbox geschrieben wurde
                {
                    int bereichID;
                    if (SmartHomeBereiche == null)//check ob bereits bereiche vorhanden sind
                    {
                        bereichID = 1;
                    }
                    else
                    {
                        bereichID = SmartHomeBereiche.Count + 1;
                    }
                    DB.insert_Bereich(bereichID, toolStripTextBox1.Text, "NoImage.png");//Smart Home Bereich in datenbank eintragen
                    toolStripTextBox1.Text = "";
                    Refresh_Bereiche();//Aufruf Bereiche refresh
                }
            }
        }
        void Refresh_Bereiche()//Funktion zum aktualisieren der vorhandenen Smart Home Bereiche
        {
            cb_bereiche.Items.Clear();//Combobox leeren
            SmartHomeBereiche = DB.get_bereiche();//Bereiche aus Datenbank holen

            if (SmartHomeBereiche != null && SmartHomeBereiche.Count > 0)//check ob Bereiche geholt wurden und die Anzahl größer als 0 ist
            {
                foreach (SmartHomeBereich sb in SmartHomeBereiche)//Loop durch Bereiche
                {
                    cb_bereiche.Items.Add(sb.Name);//aktuellen Bereich der Combobox hinzufügen
                }
                placeableImageBox1.BackgroundImage = SmartHomeBereiche[0].Bild;//Huntergrund Bild der aktuellen Ebene auf die Imagebox legen
                placeableImageBox1.BackgroundImageLayout = ImageLayout.Stretch;//Größe des hintergrundbildes an Größe der Imagebox anpassen
                akt_ebene = SmartHomeBereiche[0];//akt_ebene zuweisen um später darauf zugreifen zu könnnen
            }
            else
            {
                akt_ebene = new SmartHomeBereich(this.BackgroundImage, null, 0, "noFile.png");//temporäre Ebene erstellen falls Liste leer ist oder Daten nicht aus der Datenbank geholt werden konnten
            }
        }
        private void but_neue_ebene_MouseClick(object sender, MouseEventArgs e)//Funktion zum Context Menü öffnen um Namen für neue Ebene einzugeben
        {
            if (but_neue_ebene.ForeColor == Color.Red)//check ob Textfarbe des Button rot ist
            {
                but_neue_ebene.ForeColor = Color.Black;//Textfarbe des Button auf schwarz ändern
            }
            Ebenen_Eingabe.Show((Button)sender, e.Location);//ContextMenu an Maus Position anzeigen
        }
        private void cb_bereiche_SelectedIndexChanged(object sender, EventArgs e)//Funktion um ausgewählte Ebene anzuzeigen
        {
            akt_ebene = SmartHomeBereiche[cb_bereiche.SelectedIndex];//akt_ebene setzen um später darauf zugreifen zu können
            placeableImageBox1.BackgroundImage = akt_ebene.Bild;//Hintergrundbild für Imagebox setzen
            placeableImageBox1.BackgroundImageLayout = ImageLayout.Stretch;//Größe des Hintergrundbildes an größe der Imagebox anpassen
            reload_client_buttons();//Aufruf Refresh angezeigte Client Buttons
        }
        void reload_client_buttons()//Funktion zum anzeigen der Client Buttons für die aktuell ausgewählte Ebene
        {
            placeableImageBox1.Controls.Clear();//alle Buttons aus der Imagebox entfernen

            List<List<string>> clients_in_db = DB.get_clients();//Clients aus der Datenbank holen
            if (clients_in_db != null && clients_in_db.Count > 0)//check ob Clients aus der Datenbank geholt wurden und ob Anzahl der Clients größer als 0 ist
            {
                foreach (List<string> smc in clients_in_db)//Loop durch die vorhandenen Clients
                {
                    if (Convert.ToInt16(smc[4]) > 0 && Convert.ToInt16(smc[5]) > 0)//check ob Button Koordinaten nicht jeweils 0 sind(passiert wenn der button z.b. gelöscht wurde)
                    {
                        if (Convert.ToInt16(smc[3]) == akt_ebene.ID)//check ob aktueller Client auf der aktuellen Ebene liegt
                        {
                            Button smc_but = new Button();// neuen Button erstellen
                            smc_but.AccessibleName = smc[1];//Name des Button an IP des aktuellen Client anpassen
                            smc_but.Text = "S";//angezeigter Text auf dem Button
                            bool isActive = false;
                            if (clientlist != null)//check ob ClientList vorhanden ist(ob der Server schon läuft und Clients bereits verbunden sind)
                            {
                                foreach (SmartHomeClient sc in clientlist)//Loop durch ClientList
                                {
                                    if (((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString() == smc[1])//check ob IP identisch mit dem Ergebnis ist
                                    {
                                        isActive = true;//bestätigung das Client zum Button bereits verbunden ist
                                    }
                                }
                            }
                            if (!isActive)//check ob Client nicht verbunden ist
                            {
                                smc_but.ForeColor = Color.Red;//Textfarbe für Button auf Rot ändern
                            }
                            smc_but.Width = 20;
                            smc_but.Height = 20;
                            smc_but.Location = new Point(Convert.ToInt16(smc[4]), Convert.ToInt16(smc[5]));//Button auf der ImageBox platzieren
                            smc_but.Click += new EventHandler(OnSmartHomeButtonClick);//Funktion zuweisen die bei Click ausgeführt wird
                            smc_but.KeyDown += new KeyEventHandler(OnSmartHomeButtonDown);//Funktion zuweisen die bei Tastendruck ausgeführt wurd
                            Button_List.Add(smc_but);//Button der buttonList hinzufügen
                            placeableImageBox1.Controls.Add(smc_but);//Button der ImageBox zuweisen
                        }
                    }
                }
            }

        }
        public List<List<string>> get_CommandList(string IP)//Blanke commands für angegebenen Client aus der Datenbank holen
        {
               return DB.fill_client_commands(IP);//Commands aus Datenbank holen und zurückgeben
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (server_running)
                {
                    tcp_server.Stop();
                    server_anouncer.Close();
                    server_running = false;
                    doDummyClientConnect();
                    clientlist = null;
                }
                    udp_anouncer_active = false;
                    udp_discovery.AutoReset = false;
                Web.Stop();
                RC.StopRemoteControl();
                NL.CloseLogFile();

            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim schließen des Servers (" + ex.ToString() + ")");
                NL.CloseLogFile();
            }
        }

        void doDummyClientConnect()
        {
            TcpClient dummyClient = new TcpClient();
            dummyClient.Connect("localhost", 31337);
            dummyClient.Close();
        }
    }
}
