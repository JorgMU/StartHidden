using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace StartHidden
{
  public partial class Form1 : Form
  {
    private static readonly string _programFullname = Application.ExecutablePath;
    private static readonly string _programName = Path.GetFileName(_programFullname).Replace(".EXE",".exe");

    private static readonly string _logFullname = Path.Combine(
      Environment.ExpandEnvironmentVariables("%TEMP%"),
      Path.GetFileNameWithoutExtension(_programName) + ".log");
    
    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      LogWriteLine("{0} - jorgie@missouri.edu", _programName);
      LogWriteLine("START: " + DateTime.Now.ToString("s").Replace("T", " @ "));
      LogWriteLine("EXE: " + Application.ExecutablePath);
      LogWriteLine("CDD:  " + Environment.CurrentDirectory);

      string[] args = Environment.GetCommandLineArgs();

      if (args.Length < 2)
        LogWriteLine("No paramaters found.");
      else  {
        string cmd = args[1];
        string rest = "";

        if (cmd.Equals("?") || cmd.Equals("/?") || cmd.Equals("/h"))
        {
          string msg = string.Format( 
            "Just pass a command to run on the command line:\n\n" +
            "{0} c:\\path\\program.exe opt1 opt2\n\n" +
            "LOG: {1}",
            _programName,
            _logFullname
          );
          MessageBox.Show(msg,"Using ShowHidden");
          LogWriteLine();
          Environment.Exit(0);
        }

        if (args.Length > 1)
          for (int i = 2; i < args.Length; i++)
          {
            if (Regex.IsMatch(args[i], @"\W"))
              rest += string.Format(" \"{0}\"", args[i]);
            else rest += " " + args[i];
          }

        LogWriteLine("CMD: {0}\r\nOPT: [{1}]", cmd, rest);

        if (!cmd.ToLower().Contains("!debug"))
          StartProcessHidden(cmd, rest);
      }

      LogWriteLine();

      Environment.Exit(0);
    }

    private void StartProcessHidden(string Command, string Options)
    {
      ProcessStartInfo psi = new ProcessStartInfo(Command, Options);
      psi.LoadUserProfile = true;
      psi.UseShellExecute = true;
      psi.WindowStyle = ProcessWindowStyle.Hidden;

      Process p = new Process();
      p.StartInfo = psi;

      string msg = "";
      try { p.Start(); }
      catch (System.Exception se)
      {
        msg = "Error: " + se.Message;
        if (se.InnerException != null)
          msg += "\r\n" + se.InnerException.Message;
        LogWrite(msg);
        p = null;
      }

      if (p != null)
      {
        LogWrite("PID: " + p.Id);
        System.Threading.Thread.Sleep(1000);
        if (!p.HasExited) LogWriteLine(" - still running");
        else LogWriteLine(" - finsihed");
      }

      if(msg != "") LogWriteLine(msg);
    }

    private void LogWriteLine() { LogWrite("\r\n"); }

    private void LogWriteLine(string Message) { LogWrite(Message + "\r\n"); }

    private void LogWriteLine(string Message, params object[] Objects)
    {
      LogWriteLine(string.Format(Message, Objects));
    }

    private void LogWrite(string Message, params object[] Objects)
    {
      LogWrite(string.Format(Message, Objects));
    }

    private void LogWrite(string Message)
    {
      StreamWriter sw = null;
      try { sw = File.AppendText(_logFullname); }
      catch { }
      if (sw == null) return; //silently fail 
      sw.Write(Message);
      sw.Close();
    }
  }
}
