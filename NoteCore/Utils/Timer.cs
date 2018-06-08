using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace NoteCore.Utils
{
	/// <summary>
	/// This class is a timer that performs some tasks periodically.
	/// </summary>
	public class Timer
	{
	    [DllImport("kernel32.dll")]
	    private static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset,
	                                                             string lpTimerName);

	    [DllImport("kernel32.dll", SetLastError = true)]
	    [return: MarshalAs(UnmanagedType.Bool)]
	    private static extern bool SetWaitableTimer(SafeWaitHandle hTimer, [In] ref long pDueTime, int lPeriod,
	                                                IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine,
	                                                bool fResume);

	    public event EventHandler Woken;
	    private readonly BackgroundWorker _bgWorker = new BackgroundWorker();

	    public Timer(DateTime time)
	    {
	        _bgWorker.DoWork += BgWorkerDoWork;
	        _bgWorker.RunWorkerCompleted += BgWorkerRunWorkerCompleted;
	        Start(time);
	    }

	    public void Start(DateTime time)
	    {
	        _bgWorker.RunWorkerAsync(time.ToFileTime());
	    }

	    public void Stop()
	    {
	        _bgWorker.CancelAsync();
	    }

	    private void BgWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	    {
	        if (Woken != null)
	        {
	            Woken(this, new EventArgs());
	        }
	    }
		private void BgWorkerDoWork(object sender, DoWorkEventArgs e)
		{
			var waketime = Convert.ToInt64(e.Argument);
			using (var handle = CreateWaitableTimer(IntPtr.Zero, true, GetType().Assembly.GetName().Name + "Timer"))
			{
			    if (SetWaitableTimer(handle, ref waketime, 0, IntPtr.Zero, IntPtr.Zero, true))
			    {
			        using (var wh = new EventWaitHandle(false, EventResetMode.AutoReset))
			        {
			            wh.SafeWaitHandle = handle;
			            wh.WaitOne();
			        }
			    }
			    else
			    {
			        throw new Win32Exception(Marshal.GetLastWin32Error());
			    }
			}
		}
	}
}
