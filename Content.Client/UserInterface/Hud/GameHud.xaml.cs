using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Hud;

[GenerateTypedNameReferences]
public partial class GameHud : Control
{
    public int PlayerOneScore
    {
        set => LabelPlayerOneScore.Text = value.ToString();
    }

    public string PlayerOneName
    {
        set => LabelPlayerOneName.Text = value;
    }
        
    public int PlayerTwoScore
    {
        set => LabelPlayerTwoScore.Text = value.ToString();
    }

    public string PlayerTwoName
    {
        set => LabelPlayerTwoName.Text = value;
    }

    public string WinnerLabelText
    {
        set => WinnerLabel.Text = value;
    }
}