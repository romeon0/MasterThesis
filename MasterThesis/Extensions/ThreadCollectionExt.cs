using System.Collections.Generic;
using System.Threading;

namespace Neuroevolution_Application.Extensions
{
    static class ThreadCollectionExt
    {
        public static void WaitAll(this IEnumerable<Thread> threads)
        {
            if (threads != null)
            {
                foreach (Thread thread in threads)
                {
                    thread.Join(); 
                }
            }
        }

        public static void StartAll(this IEnumerable<Thread> threads)
        {
            if (threads != null)
            {
                foreach (Thread thread in threads)
                {
                    thread.Start();
                }
            }
        }

        public static void AbortAll(this IEnumerable<Thread> threads)
        {
            if (threads != null)
            {
                foreach (Thread thread in threads)
                {
                    thread.Abort();
                }
            }
        }
    }
}
