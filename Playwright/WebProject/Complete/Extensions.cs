using System.Text;

namespace Complete;

public static class Extensions
{
    public static string ToExceptionString(this Exception ex)
    {
        StringBuilder sb = new();
        Exception? temp = ex;
        int indent = 0;

        while (temp != null)
        {
            if (indent > 0)
            {
                sb.Append($"{new string(' ', indent)}-> ");
            }

            sb.AppendLine(temp.Message);
            indent += 2;
            temp = temp.InnerException;
        }

        return sb.ToString();
    }
}
