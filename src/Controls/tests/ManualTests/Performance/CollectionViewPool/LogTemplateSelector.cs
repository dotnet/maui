using PoolMath.Data;
using PoolMathApp.Xaml.TimelineViews;

namespace PoolMathApp.Xaml
{
	public class LogTemplateSelector : DataTemplateSelector
	{
		public DataTemplate TestLogTemplate { get; set; }
		public DataTemplate NoteLogTemplate { get; set; }
		//public DataTemplate PumpScheduleChangeLogTemplate { get; set; }
		public DataTemplate ChemicalLogTemplate { get; set; }
		public DataTemplate MaintenanceLogTemplate { get; set; }
		public DataTemplate CostLogTemplate { get; set; }

		public LogTemplateSelector()
		{
			TestLogTemplate = new DataTemplate(typeof(TestLogTimelineView));
			NoteLogTemplate = new DataTemplate(typeof(NoteLogTimelineView));
			ChemicalLogTemplate = new DataTemplate(typeof(ChemicalLogTimelineView));
			MaintenanceLogTemplate = new DataTemplate(typeof(MaintenanceLogTimelineView));
			CostLogTemplate = new DataTemplate(typeof(CostLogTimelineView));
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var logType = string.Empty;
			DataTemplate dt = default;

			if (item is Log log)
			{
				logType = log?.Type ?? string.Empty;

				switch (logType)
				{
					case TestLog.TYPE_NAME:
						dt = TestLogTemplate;
						break;
					case ChemicalLog.TYPE_NAME:
						dt = ChemicalLogTemplate;
						break;
					case MaintenanceLog.TYPE_NAME:
						dt = MaintenanceLogTemplate;
						break;
					case NoteLog.TYPE_NAME:
						dt = NoteLogTemplate;
						break;
					//case PumpScheduleChangeLog.TYPE_NAME:
					//	dt = PumpScheduleChangeLogTemplate;
					//	break;
					case CostLog.TYPE_NAME:
						dt = CostLogTemplate;
						break;
				}
			}
			return dt ?? NoteLogTemplate;
		}
	}
}
