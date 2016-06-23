using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Provider;
using Android.Database;
using Android.Util;

namespace GetFreeCalendarSpace {
	[Activity(Label = "GetFreeCalendarSpace", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity {
		

		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get calendar info
			var calendarsUri = CalendarContract.Events.ContentUri;


			///This chunk of code displays all calendars on the phone in a list
			//List the shit you want
			string[] calendarsProjection = {
											    CalendarContract.Events.InterfaceConsts.Id,
												CalendarContract.EventsColumns.Title,
												CalendarContract.Events.InterfaceConsts.Dtstart,
												CalendarContract.Events.InterfaceConsts.Dtend
												
										   };
			//list the shit you wanna display
			string[] sourceColumns = {
										 CalendarContract.EventsColumns.Title,
										 CalendarContract.Events.InterfaceConsts.Dtstart,
										 CalendarContract.Events.InterfaceConsts.Dtend
									 };
			//list the things you're gonna display your shit with
			int[] targetResources = {
										Resource.Id.evTitle,
										Resource.Id.evStart,
										Resource.Id.evEnd
									};

			var loader = new CursorLoader(this, calendarsUri, calendarsProjection, null, null, null);
			var cursor = (ICursor)loader.LoadInBackground(); //runs asynch

			///find events for & after today -- should do this after cursor is loaded with the data -- take care of that later
			//iterate through retrieved events; find events after 12am today
			
			//start at first item
			cursor.MoveToFirst();

			//new cursor to hold selected events
			MatrixCursor selectedEventsCursor = new MatrixCursor(calendarsProjection);

			//iterate through items
			while (cursor.MoveToNext()) {
				//get date for this event (start & end Date/Time in tick format)
				DateTime sDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(cursor.GetLong(2)).ToLocalTime();//index 2 is start date; 3 is end date
				DateTime eDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(cursor.GetLong(3)).ToLocalTime();

				DateTime tomorrow = DateTime.Today.ToLocalTime().AddDays(1);
				DateTime now = DateTime.Now.ToLocalTime(); //12AM today

				

				if (sDate >= now && sDate < tomorrow) {
					Log.WriteLine(Android.Util.LogPriority.Info, "sDate", sDate.ToString());
					//add item to selected events
					selectedEventsCursor.AddRow(new Java.Lang.Object[] { cursor.GetString(0), cursor.GetString(1), eDate.ToLocalTime().ToString(), sDate.ToLocalTime().ToString()});
				}
			}
			

			SimpleCursorAdapter adapter = new SimpleCursorAdapter(this, Resource.Layout.CalListItem, selectedEventsCursor, sourceColumns, targetResources);


			ListView list = FindViewById<ListView>(Resource.Id.listView1);
			list.Adapter = adapter;

			
		}
	}
}

