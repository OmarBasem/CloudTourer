using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Gtk;



public partial class MainWindow : Gtk.Window
{

    // Variables needed by the browser
    private ArrayList history, favs;
    private Window historyWin, favsWin, editWin;
    private VBox historyBox, favsBox, editBox;
    private Entry editEntry;
    private Button doneButton;
    private String currentUrl, homePage;
    private StreamWriter historyWriter, favsWriter;
    private StreamReader historyReader, favsReader, homeReader;

    
    // do all variables initializations inside the constructor
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        history = new ArrayList();
        favs = new ArrayList();
        historyWin = new Window("History");
        favsWin = new Window("Favourites");
        editWin = new Window("Add to Favourites");
        historyBox = new VBox();
        favsBox = new VBox();
        editBox = new VBox();
        historyWriter = new StreamWriter(System.Environment.CurrentDirectory + "/history.txt", true);
        historyReader = new StreamReader(System.Environment.CurrentDirectory + "/history.txt");
        favsWriter = new StreamWriter(System.Environment.CurrentDirectory + "/favs.txt", true);
        favsReader = new StreamReader(System.Environment.CurrentDirectory + "/favs.txt");
        homeReader = new StreamReader(System.Environment.CurrentDirectory + "/home.txt");
        historyWriter.AutoFlush = true;
        homePage = "google.com";
        loadItems();
        currentUrl = homePage;
        goToUrl(currentUrl, true);
        historyWin.SetPosition(WindowPosition.Center);
        editEntry = new Entry();
        editWin = new Window("Add to Favourites");
        editWin.SetPosition(WindowPosition.Center);
        editWin.SetSizeRequest(300, 100);
        editBox = new VBox();
        doneButton = new Button();

        
    }

    // loads history, favourites and homepage on browser startup
    private void loadItems()
    {
        string line;
        int i = 0;
        String key = "";
        while ((line = historyReader.ReadLine()) != null) // load history
        {
            if (i % 2 == 0)
                key = line;
            else
            {
                KeyValuePair<String, String> item = new KeyValuePair<String, String>(key, line);
                if (!history.Contains(item))
                    history.Add(item);
            }
            i++;
        }
        history.Reverse(); // latest history on top

        i = 0;
        while ((line = favsReader.ReadLine()) != null) // load favs
        {
            if (i % 2 == 0)
                key = line;
            else
            {
                KeyValuePair<String, String> item = new KeyValuePair<String, String>(key, line);
                favs.Add(item);
            }
            i++;
        }
        favs.Reverse(); // latest favs on top

        while ((line = homeReader.ReadLine()) != null) // load homepage
        {
            homePage = line;
        }
    }

    // check if a favourite item exists
    private bool favExists(String url)
    {
        foreach (KeyValuePair<String, String> item in favs)
        {
            if (item.Value == url)
                return true;
        }
        return false;
    }

    // Returns the status/error code + message
    private String getStatusCode(String code)
    {
        Console.WriteLine(code);
        String statusCode;
        switch (code)
        {
            case "OK":
                statusCode = "200 OK";
                break;
            case "BadRequest":
                statusCode = "400 Bad Request";
                break;
            case "Forbidden":
                statusCode = "403 Forbidden";
                break;
            case "NotFound":
                statusCode = "404 Not Found";
                break;
            case "InternalServerError":
                statusCode = "500 Internal Server Error";
                break;
            case "BadGateway":
                statusCode = "502 Bad Gateway";
                break;
            case "ServiceUnavailable":
                statusCode = "503 Servie Unavailable";
                break;
            default:
                statusCode = "Unknown Error";
                break;
        }
        return statusCode;
    }

    // makes http request to a url, and updates history if needed
    private void goToUrl(String url, bool updateHistory)
    {
        Console.WriteLine("NAVIGATING");
        string html = string.Empty;
        if (!url.StartsWith("http"))
            url = "https://" + url;
        urlInput.Text = url;
        currentUrl = url;

        // create request object
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        // enable decompressoin
        request.AutomaticDecompression = DecompressionMethods.GZip;
        HttpWebResponse response;
        String statusCode;
        try
        {
            // get the response
            response = (HttpWebResponse)request.GetResponse();
            // Read the response as a stream
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            html = reader.ReadToEnd();
            // get the status code
            statusCode = response.StatusCode.ToString();
        }
        catch (WebException we)
        {
            // error handling
            statusCode = ((HttpWebResponse)we.Response).StatusCode.ToString();
        }


        label1.Text = getStatusCode(statusCode);
        String pageTitle = Regex.Match(html, @"<title>\s*(.+?)\s*</title>").Groups[1].Value;
        title.Text = pageTitle;

        // If there is no page title, then use the url the display name is the history
        if (pageTitle == "")
            pageTitle = url;
        label11.Text = html;
        if (updateHistory)
        {
            KeyValuePair<String, String> historyItem = new KeyValuePair<String, String>(pageTitle, url);
            // remove history item if it already exists to avoid repitition
            history.Remove(historyItem);
            // insert history item at index 0
            history.Insert(0, historyItem);


            // Write history to file
            historyWriter.WriteLine(pageTitle);
            historyWriter.WriteLine(url);
        }




    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    // On press keybaord enter
    protected void onUrlEnter(object sender, EventArgs e)
    {
        goToUrl(urlInput.Text, true);
    }

    // delegate for clicking an item in history or favourites list
    private void onClickItemDelegate(object sender, EventArgs e)
    {
        favsWin.HideAll();
        historyWin.HideAll();
        Button btn = sender as Button;
        goToUrl(btn.Name, true);
    }


    // event listener for view history button
    protected void onClickFullHistory(object sender, EventArgs e)
    {
        favsWin.HideAll(); // hide history window if it was open
        historyWin.HideAll();
        //  check whether history window is already shown
        if (!historyBox.IsMapped)
        {
            historyWin.Add(historyBox);
            historyBox.Show();
            historyWin.Show();
            foreach (KeyValuePair<String, String> item in history)
            {
                Button b = new Button();
                EventArgs args = new EventArgs();
                b.Label = item.Key;
                b.Name = item.Value;
                b.Clicked += new EventHandler(onClickItemDelegate);
                historyBox.Add(b);
                b.Show();
            }
        }

    }

    // event listener for refresh buton
    protected void onClickRefresh(object sender, EventArgs e)
    {
        goToUrl(currentUrl, true);
    }

    // add or remove item from favourites
    protected void onClickFav(object sender, EventArgs e)
    {
        if (!favExists(currentUrl))
        {
            addFav(true, currentUrl);
        }
        else
        {

            removeFav(currentUrl);
        }
    }

    // event listener for add to favourites button 
    private void addFav(bool isNew, String url)
    {
        editBox.Remove(doneButton); // remove previous button to avoid attaching multiple event handlers
        doneButton = new Button();
        if (!editBox.IsMapped)
        {
            editWin.Title = "Add to Favourites";

            if (isNew)
                editEntry.Text = title.Text;
            else
            {
                foreach (KeyValuePair<String, String> item in favs)
                {
                    if (item.Value == url)
                    {
                        editEntry.Text = item.Key;
                        doneButton.Name = item.Key;

                        break;
                    }
                }
            }
            doneButton.Label = "Done";
            if (isNew)
                doneButton.Clicked += new EventHandler(addToFavsDelegate);
            else
                doneButton.Clicked += new EventHandler(updateFavDelegate);
            editBox.Add(editEntry);
            editBox.Add(doneButton);
            editWin.Add(editBox);
            editWin.ShowAll();
        }
    }

    // delegate for adding to favourites list
    private void addToFavsDelegate(object sender, EventArgs e)
    {
        writeNewFav(currentUrl);

    }

    // function to add web page to favourites and write to favs.txt file
    private void writeNewFav(string url)
    {
        editEntry.HideAll();
        editWin.HideAll();
        favs.Insert(0, new KeyValuePair<String, String>(editEntry.Text, url));
        favsWriter.WriteLine(editEntry.Text);
        favsWriter.WriteLine(url);
        favsWriter.Flush();
    }

    // delegate for updating favourite item
    private void updateFavDelegate(object sender, EventArgs e)
    {
        editEntry.HideAll();
        editWin.HideAll();
        Button btn = sender as Button;
        KeyValuePair<String, String> selectedItem = new KeyValuePair<string, string>();
        foreach (KeyValuePair<String, String> item in favs)
        {
            if (btn.Name == item.Key)
            {
                selectedItem = item;
                break;
            }
        }
        removeFav(selectedItem.Value);
        writeNewFav(selectedItem.Value);

    }

    // function to delete a favourite item
    private void removeFav(String url)
    {
        StreamWriter overwriter = new StreamWriter(System.Environment.CurrentDirectory + "/favs2.txt", true);
        KeyValuePair<String, String> toBeRemoved = new KeyValuePair<String, String>();
        foreach (KeyValuePair<String, String> item in favs)
        {
            if (item.Value == url)
            {
                toBeRemoved = item;
            }
            else
            {
                overwriter.WriteLine(item.Key);
                overwriter.WriteLine(item.Value);
            }

        }
        favs.Remove(toBeRemoved);
        File.Replace(System.Environment.CurrentDirectory + "/favs2.txt", System.Environment.CurrentDirectory + "/favs.txt", System.Environment.CurrentDirectory + "/favsBackup.txt");
        File.Delete(System.Environment.CurrentDirectory + "/favsBackup.txt");
        overwriter.Flush();
        overwriter.Close();

        // reopen a new stream favs stream writer!
        favsWriter.Flush();
        favsWriter.Dispose();
        favsWriter.Close();
        favsWriter = new StreamWriter(System.Environment.CurrentDirectory + "/favs.txt", true);

    }

    // delegate for delete favourite item button
    protected void onDeleteFavDelegate(object sender, EventArgs e)
    {
        favsWin.HideAll();
        Button btn = sender as Button;
        btn.Hide();
        removeFav(btn.Name);

    }

    // delegate for edit favourite item button
    protected void onEditFavDelegate(object sender, EventArgs e)
    {
        favsWin.HideAll();
        Button btn = sender as Button;
        addFav(false, btn.Name);
    }

    // event listener for clicking on view favourites button
    protected void onClickAllFavs(object sender, EventArgs e)
    {
        historyWin.HideAll(); // hide history window if it was open
        //  check whether history window is shown
        if (!favsBox.IsMapped)
        {
            favsBox = new VBox();
            favsWin = new Window("Favourites");
            favsWin.SetPosition(WindowPosition.Center);
            favsWin.Add(favsBox);
            foreach (KeyValuePair<String, String> item in favs)
            {
                Console.WriteLine(item.Key);
                HBox hBox = new HBox();
                Button b = new Button();
                Button bEdit = new Button();
                Button bDelete = new Button();
                EventArgs args = new EventArgs();
                bEdit.Label = "Edit";
                bDelete.Label = "Delete";
                bDelete.Name = item.Value;
                bEdit.Name = item.Value;
                b.Label = item.Key;
                b.Name = item.Value;
                b.Clicked += new EventHandler(onClickItemDelegate);
                bDelete.Clicked += new EventHandler(onDeleteFavDelegate);
                bEdit.Clicked += new EventHandler(onEditFavDelegate);
                b.SetSizeRequest(200, 10);
                hBox.Add(b);
                hBox.Add(bEdit);
                hBox.Add(bDelete);
                favsBox.Add(hBox);

            }
            favsWin.ShowAll();
        }
    }

    // Change Add/Remove Fav depending on whether it is favourited or not
    protected void onFavsActive(object sender, EventArgs e)
    {
        if (favExists(currentUrl))
        {
            addToFavsLabel.Label = "Remove from Favourites";
        }
        else
            addToFavsLabel.Label = "Add to Favourites";
    }

    // event listener for back button
    protected void onClickBackButton(object sender, EventArgs e)
    {
        int historyIndex = 0;
        foreach (KeyValuePair<String, String> historyItem in history)
        {
            if (historyItem.Value == currentUrl)
                break;
            historyIndex++;
        }
        if (historyIndex + 1 < history.Count)
        {
            KeyValuePair<String, String> item = (KeyValuePair<String, String>)history[historyIndex + 1];
            goToUrl(item.Value, false);
        }
    }

    // event listener for forward button
    protected void onClickForwardButton(object sender, EventArgs e)
    {
        int historyIndex = 0;
        foreach (KeyValuePair<String, String> historyItem in history)
        {
            if (historyItem.Value == currentUrl)
                break;
            historyIndex++;
        }

        if (historyIndex != 0)
        {
            KeyValuePair<String, String> item = (KeyValuePair<String, String>)history[historyIndex - 1];
            goToUrl(item.Value, false);
        }

    }

    // event listener for home page button
    protected void goToHomePage(object sender, EventArgs e)
    {
        goToUrl(homePage, true);
    }

    // event listener for set home page button
    protected void setHomePage(object sender, EventArgs e)
    {
        updateHomePageUrl(currentUrl);

    }

    // event listener for edit home page button
    protected void editHomePage(object sender, EventArgs e)
    {
        editEntry.HideAll();
        editWin.HideAll();
        updateHomePageUrl(editEntry.Text);
    }

    // function to update home page
    protected void updateHomePageUrl(String url)
    {
        StreamWriter homeWriter = new StreamWriter(System.Environment.CurrentDirectory + "/home2.txt", true);
        homeWriter.WriteLine(url);
        File.Replace(System.Environment.CurrentDirectory + "/home2.txt", System.Environment.CurrentDirectory + "/home.txt", System.Environment.CurrentDirectory + "/homeBackup.txt");
        File.Delete(System.Environment.CurrentDirectory + "/homeBackup.txt");
        homeWriter.Flush();
        homeWriter.Dispose();
        homeWriter.Close();
        homePage = url;

    }

    // event listener for shortcut keys
    protected void onKeyPress(object o, KeyPressEventArgs args)
    {
        // check control key is pressed for shortcut to activate, also allow shortcut to activate when CAPS in on
        if (args.Event.State.ToString() == "ControlMask" || args.Event.State.ToString() == "LockMask, ControlMask")
        {
            switch (args.Event.Key.ToString())
            {
                case "b":
                case "B":
                    onClickBackButton(o, args);
                    break;
                case "f":
                case "F":
                    onClickFav(o, args);
                    break;
                case "h":
                case "H":
                    onClickFullHistory(o, args);
                    break;
                case "o":
                case "O":
                    goToUrl(homePage, true);
                    break;
                default:
                case "r":
                case "R":
                    goToUrl(currentUrl, true);
                    break;
                case "s":
                case "S":
                    setHomePage(o, args);
                    break;
                case "v":
                case "V":
                    onClickAllFavs(o, args);
                    break;
                case "w":
                case "W":
                    onClickForwardButton(o, args);
                    break;
                case "q":
                case "Q":
                    Application.Quit();
                    break;
                    
            }
        }
      
    }


    // event listener for clicking edit home page button
    protected void onClickEditHome(object sender, EventArgs e)
    {
        editWin.Title = "Set Home Page";
        editBox.Remove(doneButton); // remove previous button to avoid attaching multiple event handlers
        doneButton = new Button();
        if (!editBox.IsMapped)
        {

            editEntry.Text = homePage;
            doneButton.Label = "Done";
            doneButton.Clicked += new EventHandler(editHomePage);
            editBox.Add(editEntry);
            editBox.Add(doneButton);
            editWin.Add(editBox);
            editWin.ShowAll();
        }
    }

    // event listener for clear history button
    protected void onClickClearHistory(object sender, EventArgs e)
    {
        historyWin.HideAll();
        history = new ArrayList();
        File.Delete(System.Environment.CurrentDirectory + "/history.txt");
        historyWriter.Flush();
        historyWriter.Dispose();
        historyWriter.Close();
        historyWriter = new StreamWriter(System.Environment.CurrentDirectory + "/history.txt", true);
        historyWriter.AutoFlush = true;

    }
}