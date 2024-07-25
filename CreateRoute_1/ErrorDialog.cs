namespace CreateRoute_1
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using System;

    public class ErrorDialog : Dialog
    {
        private readonly Label exceptionLabel = new Label() { Height = 25 };

        public ErrorDialog(IEngine engine, string title, string message) : base(engine)
        {
            Title = title;
            exceptionLabel.Text = message;

            OkButton = new Button("OK") { Width = 150 };

            GenerateUi();
        }

        public Button OkButton { get; private set; }

        private CollapseButton CollapseButton { get; set; }

        internal void GenerateUi()
        {
            int row = -1;

            AddWidget(exceptionLabel, ++row, 0, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(OkButton, row + 1, 0);
        }
    }
}
