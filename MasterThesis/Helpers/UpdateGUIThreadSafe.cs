using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neuroevolution_Application.Helpers
{
    public class UpdateGUIThreadSafe<T> where T : Control
    {
        Action<Form, T, object> action;
        delegate void UpdateGUICallback(Form f, T ctrl, object value);

        public UpdateGUIThreadSafe(Action<Form, T, object> action)
        {
            this.action = action;
        }

        public void UpdateElement(Form form, T ctrl, object value=null)
        {
            if (ctrl.InvokeRequired)
            {
                UpdateGUICallback d = new UpdateGUICallback(UpdateElement);
                form.Invoke(d, new object[] { form, ctrl, value});
            }
            else
            {
                if (null != action)
                    action.Invoke(form, ctrl, value);
            }
        }
    }
}
