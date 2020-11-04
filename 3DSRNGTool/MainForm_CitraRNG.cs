namespace Pk3DSRNGTool
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using Citra;
    using Magnetosphere;

    public partial class MainForm : Form
    {
        private CitraTranslator _citra;
        private Bot _citraWindow;
        private bool _connected;

        private uint _initialSeed;
        private GameState _gameState;

        private Thread _updateFramesThread;

        private static readonly object LockObject = new object();

        private void B_CitraConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate if game is supported
                var validGames = new[] { "Ultra Sun", "Ultra Moon" };
                var gameVersion = Gameversion.SelectedItem.ToString();

                if (!validGames.Any(g => string.Equals(g, gameVersion, StringComparison.InvariantCultureIgnoreCase)))
                {
                    MessageBox.Show($"Game '{gameVersion}' is not (yet) supported!");
                    return;
                }

                if (_connected)
                {
                    DisconnectCitra();
                }
                else
                {
                    ConnectCitra();
                }

                B_CitraConnect.Text = _connected ? "Disconnect" : "Connect";

                GB_MainRNG.Enabled = _connected;
                GB_EggRNG.Enabled = Method == 3 && _connected;
            }
            catch (Exception)
            {
                MessageBox.Show("Couldn't connect to Citra");
            }
        }

        private void B_CitraViewPokemons_Click(object sender, EventArgs e)
        {
            var citraViewForm = new CitraViewForm(_gameState);
            citraViewForm.Show(this);
        }

        private void B_CitraUseInitialSeed_Click(object sender, EventArgs e)
        {
            Seed.Value = CitraInitialSeed.Value;
        }

        private void B_CitraUseEggSeed_Click(object sender, EventArgs e)
        {
            St0.Value = CitraEggSeed0.Value;
            St1.Value = CitraEggSeed1.Value;
            St2.Value = CitraEggSeed2.Value;
            St3.Value = CitraEggSeed3.Value;
        }

        private void ConnectCitra()
        {
            _citraWindow = BotConfig.Citra.CreateBot();
            _citraWindow.Connect();
            _citra = (CitraTranslator)_citraWindow.Translator;

            _gameState = GetGameState(_citra);

            _updateFramesThread = new Thread(UpdateFrames);
            _updateFramesThread.Start();

            _connected = true;

            B_CitraConnect.Text = "Disconnect";
            GB_MainRNG.Enabled = true;
        }

        private void DisconnectCitra()
        {
            _updateFramesThread.Abort();

            _citraWindow.Disconnect();
            _citraWindow = null;
            _citra = null;

            _connected = false;

            B_CitraConnect.Text = "Connect";
            GB_MainRNG.Enabled = false;
        }

        private void B_CitraUseFrame_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(CitraFrame.Text, out var frame))
                Frame_min.Value = frame;
        }

        private void UpdateFrames()
        {
            try
            {
                while (true)
                {
                    lock (LockObject)
                    {
                        UpdateInterface();
                        Thread.Sleep((int)(CitraInterval.Value * 1000));
                    }

                    Thread.Sleep(1);
                }
            }
            catch (ThreadAbortException)
            {
                // Stopped thread
                MessageBox.Show("Connection with Citra has been terminated");
            }
            catch (Exception)
            {
                DisconnectCitra();
                MessageBox.Show("Disconnected, something went wrong!");
            }
        }

        private void UpdateInterface()
        {
            if (InvokeRequired)
            {
                Invoke(new ThreadStart(UpdateInterface));
            }
            else
            {
                _gameState.Update();

                _initialSeed = _gameState.InitialSeed;

                CitraInitialSeed.Value = _initialSeed;
                CitraCurrentSeed.ValueUlong = _gameState.CurrentSeed;
                CitraFrame.Text = _gameState.FrameCount.ToString("N0");
                CitraFrameDifference.Text = _gameState.FrameDifference.ToString("N0");

                L_CitraEggReadyYesNo.Text = _gameState.EggReady ? "Egg ready" : "No egg yet";

                CitraEggSeed0.Value = _gameState.EggSeeds[0];
                CitraEggSeed1.Value = _gameState.EggSeeds[1];
                CitraEggSeed2.Value = _gameState.EggSeeds[2];
                CitraEggSeed3.Value = _gameState.EggSeeds[3];
            }
        }

        private GameState GetGameState(IDeviceRW citra)
        {
            var gameVersion = Gameversion.SelectedItem.ToString();

            switch (gameVersion)
            {
                case "Ultra Sun":
                case "Ultra Moon":
                    return new GameStateUltraSunUltraMoon(citra);
                default:
                    throw new NotImplementedException($"Game '{gameVersion}' is not (yet) supported!");
            }
        }
    }
}
