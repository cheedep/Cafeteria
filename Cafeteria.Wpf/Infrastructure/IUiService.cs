using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace Cafeteria.Wpf.Infrastructure
{
    public interface IUiService
    {
        T GetView<T>();
        Task<MessageDialogResult> ShowYesNoQuestionAsync(string question, string title);
    }
}
