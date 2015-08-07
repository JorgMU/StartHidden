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
    private static readonly string _exeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

    private static readonly string _logFullname = Path.Combine(
      Environment.ExpandEnvironmentVariables("%TEMP%"),
      Path.GetFileNameWithoutExtension(_exeName) + ".log");
    
    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      LogWriteLine("StartHidden - jorgie@missouri.edu ({0})", _exeName);
      LogWriteLine("START: " + DateTime.Now.ToString("s").Replace("T", " @ "));
      LogWriteLine("EXE: " + Application.ExecutablePath);
      LogWriteLine("CD:  " + Environment.CurrentDirectory);

      string[] args = Environment.GetCommandLineArgs();

      if (args.Length < 2)
      {
        LogWriteLine("No paramaters found.");
        Environment.Exit(0);
      }

      string cmd = args[1];

      if (cmd.Equals("?") || cmd.Equals("/?") || cmd.Equals("/h"))
      {
        string msg = string.Format(
          "Just pass a command to run on the command line:\n\n" +
          "{0} c:\\path\\program.exe opt1 opt2\n\n" +
          "LOG: {1}",
          _exeName,
          _logFullname
        );
        MessageBox.Show(msg, "Using StartHidden");
        LogWriteLine(msg);
        Environment.Exit(0);
      }

      string options = "";

      if (args.Length > 2)
      {
        int index = Environment.GetCommandLineArgs()[0].Length + Environment.GetCommandLineArgs()[1].Length + 3;
        options = Environment.CommandLine.Substring(index);
      }

      LogWriteLine("CMD: {0}\r\nOPT: [{1}]", cmd, options);

      StartProcessHidden(cmd, options);

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

      try { p.Start(); }
      catch (System.Exception se)
      {
        LogWriteLine("Error: " + se.Message);
        if (se.InnerException != null) LogWriteLine(se.InnerException.Message);
        p = null;
      }
      if (p != null) LogWriteLine("PID: " + p.Id);
    }

    private void LogWriteLine() { LogWrite("\r\n"); }

    private void LogWriteLine(string Message, params object[] Objects)
    {
      LogWrite(string.Format(Message, Objects) + "\r\n");
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
