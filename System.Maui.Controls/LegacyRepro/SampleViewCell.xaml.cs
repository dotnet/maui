
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace App2
{

    public partial class SampleViewCell : ViewCell
    {
	    MenuItem _overRideItemmenuitem = null;
        public SampleViewCell()
        {
            try
            {
                InitializeComponent();

                _overRideItemmenuitem = new MenuItem();
                _overRideItemmenuitem.Text = "Over Ride";
                _overRideItemmenuitem.IsDestructive = true;
                _overRideItemmenuitem.Clicked += OverRide_Clicked;

                //this.ContextActions.Add(OverRideItemmenuitem);

                //Height = 300;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

	    void OverRide_Clicked(object sender, EventArgs e)
        {
            try
            {
                var obj = (CheckListDetails)((MenuItem)sender).BindingContext;
                obj.ChecklistStatus = CheckListStatus.OverRide;

                SetContextActions();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

	    void Checklist_tapped(object sender, EventArgs e)
        {
            try
            {
                var selectItem = (CheckListDetails)((Grid)sender).BindingContext;

                if (selectItem != null && selectItem.ChecklistStatus != CheckListStatus.OverRide)
                {
                    selectItem.ChecklistStatus = CheckListStatus.Completed;

                    SetContextActions();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //protected override void OnPropertyChanged(string propertyName = null)
        //{
        //    base.OnPropertyChanged(propertyName);
        //    if(propertyName == ViewCell.BindingContextProperty.PropertyName)
        //    {
        //        setContextActions();
        //    }
        //}
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            SetContextActions();
        }

	    void SetContextActions()
        {
            try
            {
                if (BindingContext != null)
                {
                    var obj = (CheckListDetails)BindingContext;

                    if (obj.ChecklistStatus == CheckListStatus.Default)
                    {
                        if (ContextActions.Count == 0)
                        {
                            ContextActions.Add(_overRideItemmenuitem);
                        }
                    }
                    else if (obj.ChecklistStatus == CheckListStatus.Completed)
                    {
                        if (ContextActions.Count > 0)
                        {

                            ContextActions.Remove(_overRideItemmenuitem);
                            if (_overRideItemmenuitem != null)
                            {
                                _overRideItemmenuitem.Clicked -= OverRide_Clicked;
                                _overRideItemmenuitem = null;
                            }
                        }


                    }
                    else if (obj.ChecklistStatus == CheckListStatus.Pending)
                    {
                        if (ContextActions.Count == 0)
                        {
                            ContextActions.Add(_overRideItemmenuitem);
                        }


                    }
                    else if (obj.ChecklistStatus == CheckListStatus.OverRide)
                    {
                        if (ContextActions.Count > 0)
                        {
                            ContextActions.Remove(_overRideItemmenuitem);
                            if (_overRideItemmenuitem != null)
                            {
                                _overRideItemmenuitem.Clicked -= OverRide_Clicked;
                                _overRideItemmenuitem = null;
                            }

                        }
                    }

                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
