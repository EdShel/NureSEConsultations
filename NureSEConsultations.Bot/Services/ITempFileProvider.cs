namespace NureSEConsultations.Bot.Services
{
    public interface ITempFileProvider
    {
        string GetTempFile(string extension);
    }
}
