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

			/// find free time between selected events
			//list to hold times
			JavaList<FreeTime> freetime = new JavaList<FreeTime>();
			//start looking for free time at 12AM today; the earliest anything can start today
			DateTime start = DateTime.Now.ToLocalTime();

			//iterate through items to find selected events
			while (cursor.MoveToNext()) {
				//get date for this event (start & end Date/Time in tick format)
				DateTime sDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(cursor.GetLong(2)).ToLocalTime();//index 2 is start date; 3 is end date
				DateTime eDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(cursor.GetLong(3)).ToLocalTime();
				DateTime tomorrow = DateTime.Today.ToLocalTime().AddDays(1);
				DateTime now = DateTime.Now.ToLocalTime(); //12AM today


				if (sDate >= now && sDate < tomorrow) {
					//add item to selected events
					selectedEventsCursor.AddRow(new Java.Lang.Object[] { cursor.GetString(0), cursor.GetString(1), eDate.ToString(), sDate.ToString() });

					// if event starts after our starting search point, add free time
					if (sDate > start) {
						FreeTime freeTimeChunk = new FreeTime() { Start = start };
						freeTimeChunk.End = sDate;
						freetime.Add(freeTimeChunk);

						//reset start for next iteration
						start = eDate;
					}	
				}

				// after last event, add free time from end of event until midnight, as long as event ends before midnight
				if (cursor.IsLast && eDate < DateTime.Today.AddDays(1)) {
					freetime.Add(new FreeTime() { Start = start, End = DateTime.Today.AddDays(1) });
				}
			}


			SimpleCursorAdapter adapter = new SimpleCursorAdapter(this, Resource.Layout.CalListItem, selectedEventsCursor, sourceColumns, targetResources);


			ListView list = FindViewById<ListView>(Resource.Id.listView1);
			list.Adapter = adapter;

			//list free times
			for (int i = 0; i < freetime.Size(); i++) {
				Log.WriteLine(LogPriority.Info, "freetime", "start: " + freetime[i].Start.ToLongDateString() + freetime[i].Start.ToLongTimeString());
				Log.WriteLine(LogPriority.Info, "freetime", "end: " + freetime[i].End.ToLongDateString() + freetime[i].End.ToLongTimeString());
			}
		}
	}

	// struct to store free times
	struct FreeTime {
		private DateTime start;
		private DateTime end;

		public DateTime Start {
			get {
				return start;
			}
			set {
				start = value;
			}
		}
		public DateTime End {
			get {
				return end;
			}
			set {
				end = value;
			}
		}
	};
}

