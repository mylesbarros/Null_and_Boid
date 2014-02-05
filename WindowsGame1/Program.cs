using System;

namespace WindowsGame1
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        ///
        [STAThread]
        static void Main(string[] args)
        {
            using (DaVinciExhibit game = new DaVinciExhibit())
            {
                game.Run(); // Our first mistake.
            }
        }
    }
#endif
}

