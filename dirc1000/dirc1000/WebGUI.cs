using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NICE_LOGGER;


//in dieser WebGUI Klasse werden Anfragen über Browser an den Server verarbeitet. über das WebGUI können Kommandos an die über den Server eingerichteten Clients gesendet werden
//das GUI ist über http://localhost:8844/ bzw über die IP Adresse des Computers mit dem Port 8844 erreichbar falls Port freigegeben und der Rechner von außerhalb ansprechbar ist

namespace dirc1000
{
    class WebGUI
    {
        Form1 HauptForm;
        NiceLogger N;
        bool server_running = false;
        Thread Server_Thread;
        TcpListener GUI_Listener;
        SMARTHOMEDB DB;
        string contentLength;
        public WebGUI(Form1 F, NiceLogger NL, SMARTHOMEDB D)//grundlegende WebGUI Instantierung
        {
            HauptForm = F;//
            N = NL;       //übergebene Parameter in der Klasse für späteren Zugriff bekannt machen
            DB = D;       //
            GUI_Listener = new TcpListener(IPAddress.Any, 8844);//Listener für eingehende Web Anfragen erstellen
            Server_Thread = new Thread(Start);//Thread zum WebserverStart
            Server_Thread.Start();//Start des Thread
        }
        public void Start()//Eigendlicher WebServer Start
        {
            GUI_Listener.Start();//Start des Listeners
            server_running = true;
            while (server_running)//Schleife zum bearbeiten eingehender Verbindungen
            {
                //if (GUI_Listener.Pending())//Check ob wartende Anfrage vorhanden ist
                //{
                    TcpClient client = GUI_Listener.AcceptTcpClient();//Anfrage als TCP Client behandlen
                    Thread handle_thread = new Thread(new ParameterizedThreadStart(Handle_Client));//neuen Thread für diesen neuen Client erstellen 
                    handle_thread.Start(client);//Thread starten und den gerade gefangenen Client übergeben
                                                // }
                
            }
            
        }
        public void Handle_Client(object client)//Funktion zum bearbeiten der Client Anfrage
        {
            TcpClient CL = (TcpClient)client;//Übergebenes Object wieder zum TCP Client Casten
            byte[] recv;//Bytearray für empfangene Daten erstellen
            byte[] reply;//Bytearray für Antwortdaten erstellen
            bool cl_connected = true;//Bool zum checken ob Client weiterhin connected ist(hilft um Fehler beim totladen des aufrufenden Browsers zu unterbinden)
            while (cl_connected)//Schleife zum prüfen ob Client weiterhin verbunden ist
            {
               // if (CL.Available > 0)//check ob Client verarbeitbare Daten hat
               // {
                    recv = new byte[CL.Available];//Bytearray mit der Größe der empfangbaren Daten deklarieren
                    CL.GetStream().Read(recv, 0, CL.Available);//Netzwerkstream vom client holen und auslesen
                    N.WriteToLog(LogType.Info, Encoding.UTF8.GetString(recv));//empfangene Daten in Log Datei schreiben
                    reply = process_request(recv);//Aufruf zum Daten verarbeiten
                    N.WriteToLog(LogType.Info, Encoding.UTF8.GetString(reply));//zu sendende Daten in Log Datei schreiben
                    CL.GetStream().Write(reply, 0, reply.Length);//Netzwerkstream von Client holen und rein schreiben
                    CL.GetStream().Flush();//daten im Stream sicher absenden, damit der Browser auch etwas anzeigt
                    CL.Close();//Client verbindung schließen
                    cl_connected = false;//Bool auf false setzen, damit schleife unterbrochen wird
                //}
            }
        }
        public byte[] process_request(byte[] recv)//Funktion zum verarbeiten der Daten vom Client
        {
            byte[] ret;//neues Bytearray für die zu versendenden Daten
            string daten = Encoding.ASCII.GetString(recv);//übergebene Daten in String umwandeln
            string[] splitter = new string[] { "\r\n" };//einfachste Variante meiner meinung ein String Array mit einem Wert um den daten String zu teilen
            string[] header_info = daten.Split(splitter, StringSplitOptions.None);//teilen der daten anhand des String Array(jede neue Reihe in ein Feld des neuen Array)
            string wanted_file = "";
            if (header_info[0].Split(' ').Length > 1)
            {
                wanted_file = header_info[0].Split(' ')[1];//erstes Feld im Array ist i.d.R. immer die angefragte Datei
            }
            StreamReader fileread;//Stream um Datei auf Festplatte zu lesen
            string fileContent = "";//DateiInhalt, kann auch Rückgabewert einer anderen Funktion werden
            string extention = "";//im oberen teil genutzt um übergebene Parameter zur anfrage zu speichern, im unteren Bereich genutzt für das erfassen der Datei Endung(html,JPEG,PNG o.ä.)
            int FileSize = 0;
            if (wanted_file == "/")//check ob URL ohne Datei aufgerufen wurde
            {
                wanted_file = "/index.html";//zuweisung datei index.html als fallback für den fall das keine Datei angegeben wurde
            }
            if(wanted_file.Split('=').Length > 1)//check ob Wanted File evtl mit einem Parameter angefragt wurde
            {
                extention = wanted_file.Split('=')[1];//übergenen Wert zur späteren Verwendung speichern
                wanted_file = wanted_file.Split('=')[0];//Name der angefragten Datei selbst speichern
            }
            switch (wanted_file)//je nach dem welche Datei abgefragt wurde einen entsprechenden Arbeitsgang ausführen
            {
                case "/clientlist"://Arbeitsschritt bei anfrage clientlist
                    fileContent = get_client_list();//Client List holen und in FileContent speichern
                    ret = new byte[fileContent.Length];//Array mit richtiger größe erstellen
                    ret = Encoding.UTF8.GetBytes(fileContent);//Daten als byte in das array schreiben
                    FileSize = ret.Length;//Größe der erhaltenen Daten festhalten
                    contentLength = "Content-Length: " + ret.Length.ToString() + "\r\n";//Header Feld für Daten Größe erstellen
                    break;
                case "/buttonlist"://Arbeitsschritt bei anfrage buttonlist
                    fileContent = get_button_list();//Button List holen und in FileContent speichern
                    ret = new byte[fileContent.Length];//Array mit richtiger größe erstellen
                    ret = Encoding.UTF8.GetBytes(fileContent);//Daten als byte in das array schreiben
                    FileSize = ret.Length;//Größe der erhaltenen Daten festhalten
                    contentLength = "Content-Length: " + ret.Length.ToString() + "\r\n";//Header Feld für Daten Größe erstellen
                    break;
                case "/roomlist"://Arbeitsschritt bei anfrage roomlist
                    fileContent = get_SmartHome_Ebenen();//Smart Home ebenen holen und in FileContent speichern
                    ret = new byte[fileContent.Length];//Array mit richtiger größe erstellen
                    ret = Encoding.UTF8.GetBytes(fileContent);//Daten als byte in das array schreiben
                    FileSize = ret.Length;//Größe der erhaltenen Daten festhalten
                    contentLength = "Content-Length: " + ret.Length.ToString() + "\r\n";//Header Feld für Daten Größe erstellen
                    break;
                case "/commandlist"://Arbeitsschritt bei anfrage commandlist
                    fileContent = get_command_list(extention);//Smart Home Client Commands holen und in Filecontent speichern
                    ret = new byte[fileContent.Length];//Array mit richtiger größe erstellen
                    ret = Encoding.UTF8.GetBytes(fileContent);//Daten als byte in das array schreiben
                    FileSize = ret.Length;//Größe der erhaltenen Daten festhalten
                    contentLength = "Content-Length: " + ret.Length.ToString() + "\r\n";//Header Feld für Daten Größe erstellen
                    break;
                case "/sendCommand"://Arbeitsschritt bei anfrage sendCommand
                    fileContent = send_web_command(extention);//Command an Client senden und rückgabewert in FileContent speichern
                    ret = new byte[fileContent.Length];//Array mit richtiger größe erstellen
                    ret = Encoding.UTF8.GetBytes(fileContent);//Daten als byte in das array schreiben
                    FileSize = ret.Length;//Größe der erhaltenen Daten festhalten
                    contentLength = "Content-Length: " + ret.Length.ToString() + "\r\n";//Header Feld für Daten Größe erstellen
                    break;
                default:
                    if (File.Exists(Application.StartupPath + "\\WebGUI" + wanted_file.Replace("/", "\\")))//check ob angefragte Datei überhaupt im programmverzeichnis vorhanden ist
                    {
                        fileread = new StreamReader(Application.StartupPath + "\\WebGUI" + wanted_file.Replace("/", "\\"));//Datei öffnen
                        fileContent = fileread.ReadToEnd();//Datei Inhalt komplett in FileContent einlesen
                        fileread.Close();//angefragte Datei schließen
                        fileContent = insert_content(fileContent);//Aufruf um bestimmte Datein in der eingelesenen Datei zu ersetzen
                        ret = new byte[fileContent.Length];//Bytearray mit passender größe erstellen
                        ret = Encoding.UTF8.GetBytes(fileContent);//Daten als Bytes in das Array schreiben
                        FileSize = ret.Length;//speichern der Array Größe zur weiteren Verwendung
                        contentLength = "Content-Length: " + ret.Length.ToString() + "\r\n";//Header Feld für Daten Größe erstellen
                    }
                    else
                    {
                        if (File.Exists(Application.StartupPath + "\\Bereiche" + wanted_file.Replace("/", "\\")))//check ob angefragte Datei überhaupt im programmverzeichnis vorhanden ist
                        {
                            byte[] tempret;//Byte Array deklarieren
                            tempret = File.ReadAllBytes(Application.StartupPath + "\\Bereiche" + wanted_file.Replace("/", "\\"));//daten aus Datei als Byte lesen und in Array speichern
                            ret = new byte[fileContent.Length];//Array mit daten Größe erstellen
                            ret = tempret;//daten des ersten array an zweites Array übergeben, einfachster Weg wie ich finde, um schreib aufwand zu sparen
                            FileSize = ret.Length;//daten Größe zur späteren Verwendung speichern
                            contentLength = "Content-Length: " + ret.Length.ToString() + "\r\n";//Header Feld für Daten Größe erstellen
                        }
                        else
                        {
                            fileread = new StreamReader(Application.StartupPath + "\\WebGUI\\noFile.html");//Datei einlesen die übermittelt wird, wenn angefragte Datei nicht im programmpfad vorhanden ist
                            fileContent = fileread.ReadToEnd();//Daten aus Datei in FileContent speichern
                            fileread.Close();//Datei schließen
                            fileContent = insert_content(fileContent);//daten in eingelesener Datei ersetzen
                            ret = new byte[fileContent.Length];//Array mit passender Daten Größe erstellen
                            ret = Encoding.UTF8.GetBytes(fileContent);//daten als Byte in array schreiben
                            FileSize = ret.Length;//daten Größe zur späteren Verwendung speichern
                            contentLength = "Content-Length: " + ret.Length.ToString() + "\r\n";//Header Feld für Daten Größe erstellen
                        }
                    }
                    break;
            }
            extention = wanted_file.Split('.')[wanted_file.Split('.').Length - 1];//Datei Name aufteilen um header für die entsprechende Datei Endung zu erstellen
            extention = extention.ToLower();//Datei Endung immer klein verarbeiten
            string httpReply = "HTTP/1.1 200 OK\r\n";                                   //
            string headerDate = "Date: " + DateTime.Now.ToString("r") + "\r\n";         //
            string ServerInfo = "Server: DIRC WebGUI\r\n";                              //Standard Header informationen die bei jeder Abfrage Identisch sind
            string keepAlive = "Keep-Alive: timeout=5, max=100\r\n";                    //
            string Connection = "Connection: Keep-Alive\r\n";                           //
            string ContentType = "";                                                    //
            switch (extention)//je nach dem welche Datei Endung vorhanden ist, weitere Daten für Header erstellen
            {
                case "jpg"://Arbeitsschritt für Datei Endung jpg
                    ContentType = "Accept-Ranges: bytes\r\n";//übermittelte Daten sind Bytes
                    ContentType = ContentType + "Content-Type: image/jpeg\r\n\r\n";//Typ der übermittelten Daten ist Bild/jpg
                    break;
                case "jpeg"://Arbeitsschritt für Datei Endung jpeg
                    ContentType = "Accept-Ranges: bytes\r\n";//übermittelte Daten sind Bytes
                    ContentType = ContentType + "Content-Type: image/jpeg\r\n\r\n";//Typ der übermittelten Daten ist Bild/jpeg
                    break;
                case "png"://Arbeitsschritt für Datei Endung png
                    ContentType = "Accept-Ranges: bytes\r\n";//übermittelte Daten sind Bytes
                    ContentType = ContentType + "Content-Type: image/png\r\n\r\n";//Typ der übermittelten Daten ist Bild/png
                    break;
                case "gif"://Arbeitsschritt für Datei Endung gif
                    ContentType = "Accept-Ranges: bytes\r\n";//übermittelte Daten sind Bytes
                    ContentType = ContentType + "Content-Type: image/gif\r\n\r\n";//Typ der übermittelten Daten ist Bild/gif
                    break;
                case "bmp"://Arbeitsschritt für Datei Endung bmp
                    ContentType = "Accept-Ranges: bytes\r\n";//übermittelte Daten sind Bytes
                    ContentType = ContentType + "Content-Type: image/bmp\r\n\r\n";//Typ der übermittelten Daten ist Bild/bmp
                    break;
                default://Arbeitsschritt für alle anderen Datei endungen
                    ContentType = "Content-Type: text/html\r\n\r\n";//Typ der übermittelten Daten ist Text/html
                    break;
            }
            string replyContent = httpReply + headerDate + ServerInfo + contentLength + keepAlive + Connection + ContentType;//zusammenbauen des Headers
            byte[] reply = new byte[replyContent.Length + FileSize];//Bytearray mit größe des Headers und des FileContent erstellen
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(replyContent), 0, reply, 0, replyContent.Length);//daten aus dem Header in das neue Array kopieren
            Buffer.BlockCopy(ret, 0, reply, replyContent.Length, ret.Length);//Daten aus ret in das neue Array hinter den Header kopieren
            return reply;//ergebnis zurückgeben
        }
        public string get_button_list()//Abfrage der Buttonlist
        {
            string buttons = "";//String um die Buttons aus der Buttonlist zu speichern
            List<List<string>> buttonlist = DB.get_clients();//Datenbank Abfrage zum holen der Clients
            int i = 1;
            float buttonTop; //floats um Button Position zu speichern
            float buttonLeft;//
            
            foreach(List<string> buttonrow in buttonlist)//loop durch die String Liste
            {
                buttonTop = Convert.ToInt32(buttonrow[5]);//wert in int umwandeln
                buttonLeft = Convert.ToInt32(buttonrow[4]);//wert in int umwandeln
                buttonTop = buttonTop * 0.2127659574468085f;//da wir im Form selbst die größe des Bildes auf der Imagebox anpassen, müssen wir für die Web Anzeige die Position der Buttons umrechnen
                buttonLeft = buttonLeft * 0.1302083f;       //
                if (i == buttonlist.Count)//check ob i so groß ist wie die Anzahl der buttonlist elemente
                {
                    buttons = buttons + buttonrow[1] + "-" + buttonrow[2] + "-" + buttonLeft + "-" + buttonTop + "-" + buttonrow[3];//letzten Button in den String schreiben
                }
                else
                {
                    buttons = buttons + buttonrow[1] + "-" + buttonrow[2] + "-" + buttonLeft + "-" + buttonTop + "-" + buttonrow[3] + ";";//Buttons mit Semikolon getrennt in den String schreiben
                }
                i++;//i um eins erhöhen
            }
            return buttons;//String mit allen Buttons übergeben
        }
        public string send_web_command(string commandString)//Senden der Client Kommandos über die WebGUI
        {
            string[] commands = commandString.Split('-');//übergebenes Kommando teilen um Client IP und eigendliches Kommando zu ermitteln
            string ip = commands[0];
            string comm = commands[1] + "1";//an das eigendliche Kommando eine 1 anhängen, damit der Client weis, bis wo hin er lesen muss

            foreach(SmartHomeClient SC in (ObservableCollection<SmartHomeClient>)HauptForm.get_clientlist())//Loop durch die Smart Home Clients
            {
                if(((IPEndPoint)SC._TcpClient.Client.RemoteEndPoint).Address.ToString() == ip)//check ob angegebene IP mit der des aktuellen Client übereinstimmt
                {
                    SC._TcpClient.GetStream().Write(Encoding.ASCII.GetBytes(comm), 0, comm.Length);//Kommando in den Client Stream schreiben
                    SC._TcpClient.GetStream().Flush();//Daten übermitteln und Stream bereinigen
                    
                }
            }
            return "erledigt";//Rückgabe um Browser zu signalisieren, das Kommando ausgeführt wurde
        }
        public string get_command_list(string ip)//Kommandos für den angegebenen Client holen
        {
            string ret = "";
            List<List<string>> command_list = DB.fill_client_commands(ip);//Datenbank abfrage um die Kommandos zu bekommen
            int i = 0;
            foreach(List<string> commandrow in command_list)//Loop durch die erhaltenen Kommandos
            {
                if(i == command_list.Count)//check ob i die gleiche Größe hat, wie die Anzalh der einzelnen Kommandos
                {
                    ret = ret + commandrow[0] + "-" + commandrow[1];// letztes Kommando in den String schreiben
                }
                else
                {
                    ret = ret + commandrow[0] + "-" + commandrow[1] + ";";//Kommandos mit Semikolon getrennt in den String schreiben
                }
                i++;//i erhöhen
            }
            return ret;//Rückgabe der Kommandos
        }
        public string get_SmartHome_Ebenen()//Smart Home Ebenen holen
        {
            string SHE = "";
            List<SmartHomeBereich> bereiche = DB.get_bereiche();//Datenbank Abfrage um die einzelnen Ebenen zu holen
            int i = 1;
            foreach(SmartHomeBereich SB in bereiche)//Loop durch die einzelnen Ebenen
            {
                if (i == bereiche.Count)//Check ob i die gleiche Größe hat wie die Antahl der ebenen
                {
                    SHE = SHE+ SB.ID + "-" + SB.Name + "-\"" + SB.bildName + "\"";//letze Ebene in den String schreiben
                }
                else
                {
                    SHE = SHE + SB.ID + "-" + SB.Name + "-\"" + SB.bildName + "\";";//Ebenen mit Semikolon getrennt in den String schreiben
                }
                i++;//i erhöhen
            }
            return SHE;//Ebenen übergeben
        }
        public string get_client_list()//Clients holen
        {
            string clientlist = "" ;
            int i = 1;
            ObservableCollection<SmartHomeClient> akt_liste = (ObservableCollection<SmartHomeClient>)HauptForm.get_clientlist();//Client List aus der Hauptform holen
            if (akt_liste != null)
            {
                foreach (SmartHomeClient CL in akt_liste)//Loop durch die einzelnen Clients
                {
                    if (i == akt_liste.Count)//check ob i die gleiche Größe hat, wie die Anzahl der Clients in der Liste
                    {
                        clientlist = clientlist + CL.GetId().ToString() + "-" + CL.ebene.ToString() + "-" + ((IPEndPoint)CL._TcpClient.Client.RemoteEndPoint).Address.ToString();//letzten Client in den String schreiben
                    }
                    else
                    {
                        clientlist = clientlist + CL.GetId().ToString() + "-" + CL.ebene.ToString() + "-" + ((IPEndPoint)CL._TcpClient.Client.RemoteEndPoint).Address.ToString() + ";";//Clients mit Semikolon getrennt in den String schreiben
                    }
                    i++;//i erhöhen
                }
            }
            else
            {
                clientlist = "leer";// wenn keine Clients in der Datenbank sind oder ein Problem mit der Abfrage vorhanden ist, wird als Liste "leer" zurückgegeben
            }
            return clientlist;//rückgabe der Clients
        }
        public string insert_content(string fileContent)//Funktion um definierte Platzhalter in den WebGUI Dateien mit mehr oder weniger Sinnvollen Daten zu ersetzen
        {
            string newContent = "";
            string server_running;
            if (HauptForm.server_running)//Check ob Server läuft
            {
                server_running = "Server läuft";//String für die Anzeige anstelle des Platzhalters
            }
            else
            {
                server_running = "Server aktuell nicht aktiv";//String für die Anzeige anstelle des Platzhalters
            }
            newContent = fileContent.Replace("<server_running>", server_running);//Platzhalter gegen String ersetzen(aktuell nur ein Platzhalter vorhanden"<server_running>")
            return newContent;//neue Daten ohne Platzhalter übergeben
        }
        public void Stop()//WebGUI verarbeitung stoppen
        {
            //GUI_Listener.Server.Close();
            server_running = false;
            doDummyClientConnect();
            //Server_Thread.Abort();//Server Thread abbrechen
            //GUI_Listener.Stop();//HTTP Listener anhalten
        }
        void doDummyClientConnect()
        {
            TcpClient dummyClient = new TcpClient();
            dummyClient.Connect("localhost", 8844);
            dummyClient.Close();
        }
    }
}
