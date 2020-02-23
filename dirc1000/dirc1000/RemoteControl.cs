using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NICE_LOGGER;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Forms;

//RemoteControl ist eine Klasse um externe Software als Fernsteuerung für DIRC zu verwenden 
//z.b. Smartphone Apps, Voice Recognition services oder andere Software die auf einem PC installiert ist
namespace dirc1000
{
    class RemoteControl
    {
        TcpListener RCServer;
        Thread RCThread;
        NiceLogger NL;
        Form1 Frm;
        private bool RemoteRunning;
        List<Thread> RemoteThreads = new List<Thread>();

        public void StartRemoteControl(NiceLogger N, Form1 F)// Remote Klasse initialisieren
        {
            Frm = F;// Verweis auf haupt form zur späteren Verwendung speichern
            NL = N;//Logger für Logging funktionalität zur späteren Verwendung speichern
            RCServer = new TcpListener(IPAddress.Any, 8845);//Listener mit port 8845 auf 0.0.0.0 initialisieren
            RCThread = new Thread(HandleRemoteControl);//eigenen Thread für den Listener erstellen
            RCThread.Start();//Thread starten
        }
        public void StopRemoteControl()//WebGUI verarbeitung stoppen
        {
            //GUI_Listener.Server.Close();
            RemoteRunning = false;

            doDummyClientConnect();

            foreach(Thread T in RemoteThreads)
            {
                T.Abort();
            }
            //Server_Thread.Abort();//Server Thread abbrechen
            //GUI_Listener.Stop();//HTTP Listener anhalten
        }
        void doDummyClientConnect()
        {
            TcpClient dummyClient = new TcpClient();
            dummyClient.Connect("localhost", 8845);
            dummyClient.Close();

        }
        void HandleRemoteControl()//eingehende Verbindungen verarbeiten
        {
            RCServer.Start();//Listener selbst starten
            RemoteRunning = true;//bool für den folgenden Loop
            while (RemoteRunning)//Loop für die kontinuierliche prüfung auf eingehende Verbindungen
            {
                //if (RCServer.Pending())//check ob eingehende Verbindung wartet
                //{
                TcpClient cl = RCServer.AcceptTcpClient();//Verbindung akzeptieren
                Thread RCConnection = new Thread(new ParameterizedThreadStart(OnRemoteConnect));//neuen Thread für die gerade akzeptierte Verbindung erstellen
                    RemoteThreads.Add(RCConnection);
                RCConnection.Start(cl);//Thread starten und Verbindung als TCP Client übergeben
                //}
            }
        }
        void OnRemoteConnect(object client)//verarbeiten von Anfragen der eingehenden Verbindung
        {
            TcpClient cl = (TcpClient)client;//übergebenes Object zu TCP Client casten 
            bool onGoing = true;
            while (cl.Connected)//Loop für die Abfrage von eingehenden Daten
            {
                //if (cl.Available > 0)//check ob daten vorhanden sind
                //{
                byte[] RCBuffer = new byte[cl.Available];//Byte Array mit größe der verfügbaren Daten erstellen
                cl.GetStream().Read(RCBuffer, 0, cl.Available);//Daten aus dem Stream in den Array einlesen
                string recv_command = ASCIIEncoding.ASCII.GetString(RCBuffer);//Daten in String umwandeln
                string[] splits = new string[1];//String Array für die separator des Split Kommandos erstellen
                splits[0] = ";";//Wert an dem gesplittet werden soll zuweisen
                string[] recv_tree = recv_command.Split(splits, System.StringSplitOptions.None);//Split Kommando ausführen und ergebnis wieder in String Array speichern
                if (recv_tree.Length > 1)
                {
                    switch (recv_tree[0])//Switch statement für art der erhaltenen Daten
                    {
                        case "1"://ausführen wenn Wert 1 ist
                            byte[] clients = CreateClientList();//ClientList holen
                            WriteLog(LogType.Error, "Client gesendet " + recv_command);//Log schreiben was an Daten empfangen wurde
                            cl.GetStream().Write(clients, 0, clients.Length);//ClientList in den Datenstream der Verbindung schreiben
                            cl.GetStream().Flush();//sicherstellen, dass Daten übertragen werden
                            onGoing = false;
                            break;//Switch verlassen
                        case "2"://ausführen wenn Wert 2 ist
                            byte[] Commands = CreateCommandList(recv_tree[1]);//CommandList holen
                            WriteLog(LogType.Error, "Commands für Client " + recv_tree[1] + "angefordert");// Log Schreiben für welche IP die Commands angefragt wurden
                            cl.GetStream().Write(Commands, 0, Commands.Length);//CommandList in den Datenstream der Verbindung schreiben
                            cl.GetStream().Flush();// sicherstellen, dass Daten übertragen werden
                            onGoing = false;
                            break;//Switch verlassen
                        case "3"://ausführen wenn Wert 3 ist
                            WriteLog(LogType.Error, "Command " + recv_tree[2] + "an Client " + recv_tree[1] + "senden");//Log Schreiben welcher Command für Welchen Client abgesetzt wurde
                            SendClientCommand(recv_tree[1], recv_tree[2] + "1");// Command über Funktion absetzen
                            onGoing = false;
                            break;//Switch verlassen
                        default:
                            WriteLog(LogType.Error, "Client gesendet default länge der Daten=" + RCBuffer.Length + " Daten " + recv_command);//Log schreiben was an Daten empfangen wurde
                            WriteLog(LogType.Error, "recv_tree[0] = " + recv_tree[0] + " groesse recv_tree " + recv_tree.Length);
                            cl.GetStream().Write(RCBuffer, 0, RCBuffer.Length);//empfangene Daten zurück in den Datenstream schreiben
                            cl.GetStream().Flush();//sicherstellen, dass Daten übertragen werden
                            break;//Switch verlassen
                    }
                }
                else
                {
                    if (!RemoteRunning)
                    {
                        onGoing = false;
                    }
                }

                recv_tree = null;
                if (!onGoing)
                {
                    cl.Close();
                }
                //}
            }

        }
        byte[] CreateClientList()//Liste aller verbundenen Clients holen und an die Remote übergeben
        {
            string Clientliststring = "";//intitialisieren des String der die einzelnen mit Semikolon getrennten IP Adressen hält
            int i = 1;//Counter um den String Clientliststring richtig zu erstellen
            ObservableCollection<SmartHomeClient> SCList = (ObservableCollection<SmartHomeClient>)Frm.get_clientlist();//ClientList selbst holen
            foreach (SmartHomeClient CL in SCList)//Loop durch die ClientList
            {
                if (i == SCList.Count)//check ob i so Groß ist wie die Anzahl der Clients in der Liste
                {
                    Clientliststring = Clientliststring + CL.ClientName + "-" + ((IPEndPoint)CL._TcpClient.Client.RemoteEndPoint).Address.ToString();// letzten Client an den Clientliststring anhängen
                }
                else
                {
                    Clientliststring = Clientliststring + CL.ClientName + "-" + ((IPEndPoint)CL._TcpClient.Client.RemoteEndPoint).Address.ToString() + ";";// Client an den Clientliststring mit Semikolon getrennt anhängen
                }
                i++;//i um 1 erhöhen
            }
            return Encoding.ASCII.GetBytes("2#" + Clientliststring + ";\n");//entgültigen Clientliststring als Byte Array zurückgeben
        }
        byte[] CreateCommandList(string IP)// Liste aller Commands für den angegebenen Client erstellen und zürckgeben
        {
            string CommandListString = "";// CommandListString leer initialisieren
            int i = 1;// Counter um den String CommandListString richtig zu erstellen
            List<List<string>> commands = Frm.get_CommandList(IP);// Commands anhand der übergebenen IP aus der Datenbank holen
            foreach (List<string> commandrow in commands)//Loop durch die einzelnen ergebnis Reihen
            {
                string singleCommand = IP + "-" + commandrow[0] + "-" + commandrow[1];// einzelnen Command als String erstellen
                if (i < commands.Count)// check ob i kleiner als die Anzahl der Commands ist
                {
                    CommandListString = CommandListString + singleCommand + ";";// einzelnen Command an den CommandListString mit semikolon getrennt anhängen
                }
                else
                {
                    CommandListString = CommandListString + singleCommand;// letzen Command an den CommandListString anhängen
                }
                i++;//i um 1 erhöhen
            }
            return Encoding.ASCII.GetBytes("3#" + CommandListString + "\n");//CommandListString als Byte Array zurückgeben
        }
        void WriteLog(LogType LType, string message)//funktion für den selbst geschriebenen Logger, um Log dateien im Programm Startverzeichniss abzulegen.
        {
            NL.WriteToLog(LType, message);
        }
        void SendClientCommand(string IP, string command_value)//Funktion zum Command senden an Verbundenen Client
        {

            ObservableCollection<SmartHomeClient> clientlist = (ObservableCollection<SmartHomeClient>)Frm.get_clientlist();
            foreach (SmartHomeClient sc in clientlist)//Loop durch die ClientList
            {
                if (((IPEndPoint)sc._TcpClient.Client.RemoteEndPoint).Address.ToString() == IP)//check ob IP mit dem aktuellen Client übereinstimmt
                {
                    byte[] send_daten = new byte[command_value.Length];//Bytearray zum senden erstellen
                    Encoding.ASCII.GetBytes(command_value, 0, command_value.Length, send_daten, 0);// Command in Bytearray umwandeln
                    sc._TcpClient.GetStream().Write(send_daten, 0, command_value.Length);//Command in Stream schreiben
                    sc._TcpClient.GetStream().Flush();//stream Flush um senden der Daten sicher zu stellen
                }

            }
        }
    }
}
