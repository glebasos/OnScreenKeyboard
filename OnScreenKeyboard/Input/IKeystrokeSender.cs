namespace OnScreenKeyboard.Input;

public interface IKeystrokeSender
{
    void SendText(string text);
    void SendBackspace();
}
