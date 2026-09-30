#nullable enable
using System.Collections.Generic;
using System.Runtime.Versioning;
using Android.App;
using Android.App.Assist;
using Android.OS;
using Android.Runtime;
using Android.Service.Autofill;
using Android.Views.Autofill;
using Android.Widget;

namespace Maui.Controls.Sample.Platform;

/// <summary>
/// Autofill service that offers a fill dialog for the Entries of Issue38819 and nothing else.
/// </summary>
/// <remarks>
/// It only runs while the Issue38819 UI test makes it the device's autofill service. On API 33+ TextView asks
/// AutofillManager whether to show the fill dialog instead of the soft keyboard from its tap handling, which is
/// skipped when the text layout is invalidated by a font change on focus.
/// </remarks>
[SupportedOSPlatform("android26.0")]
[Service(Exported = true, Permission = "android.permission.BIND_AUTOFILL_SERVICE")]
[IntentFilter(new[] { "android.service.autofill.AutofillService" })]
[Register("com.microsoft.maui.uitests.Issue38819AutofillService")]
public class Issue38819AutofillService : AutofillService
{
	public const string AutofillHint = "maui-issue-38819";
	public const string DialogHeader = "Issue38819 fill dialog";
	public const string DatasetLabel = "Issue38819 suggestion";
	public const string DatasetValue = "Autofilled";

	public override void OnFillRequest(FillRequest request, CancellationSignal cancellationSignal, FillCallback callback)
	{
		var autofillIds = FindAutofillIds(request);

		if (!OperatingSystem.IsAndroidVersionAtLeast(33) || autofillIds.Count == 0)
		{
			callback.OnSuccess(null);
			return;
		}

		var suggestion = CreatePresentation(DatasetLabel);
		var dataset = new Dataset.Builder();

		foreach (var autofillId in autofillIds)
		{
			dataset.SetField(autofillId, new Field.Builder()
				.SetValue(AutofillValue.ForText(DatasetValue)!)
				.SetPresentations(new Presentations.Builder()
					.SetMenuPresentation(suggestion)
					.SetDialogPresentation(suggestion)
					.Build())
				.Build());
		}

		callback.OnSuccess(new FillResponse.Builder()
			.AddDataset(dataset.Build())
			.SetDialogHeader(CreatePresentation(DialogHeader))
			.SetFillDialogTriggerIds(autofillIds.ToArray())
			.Build());
	}

	public override void OnSaveRequest(SaveRequest request, SaveCallback callback) => callback.OnSuccess();

	static List<AutofillId> FindAutofillIds(FillRequest request)
	{
		var autofillIds = new List<AutofillId>();
		var structure = request.FillContexts[^1].Structure;

		for (var i = 0; i < structure.WindowNodeCount; i++)
		{
			if (structure.GetWindowNodeAt(i)?.RootViewNode is AssistStructure.ViewNode root)
			{
				FindAutofillIds(root, autofillIds);
			}
		}

		return autofillIds;
	}

	static void FindAutofillIds(AssistStructure.ViewNode node, List<AutofillId> autofillIds)
	{
		if (node.GetAutofillHints()?.Contains(AutofillHint) is true && node.AutofillId is AutofillId autofillId)
		{
			autofillIds.Add(autofillId);
		}

		for (var i = 0; i < node.ChildCount; i++)
		{
			if (node.GetChildAt(i) is AssistStructure.ViewNode child)
			{
				FindAutofillIds(child, autofillIds);
			}
		}
	}

	RemoteViews CreatePresentation(string text)
	{
		var presentation = new RemoteViews(PackageName, Android.Resource.Layout.SimpleListItem1);
		presentation.SetTextViewText(Android.Resource.Id.Text1, text);
		return presentation;
	}
}
