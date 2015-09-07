using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Twilio.Web.Controllers
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Provides a ForEach functionality for the IEnumerable interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
    }

    public class ApplicationMessage
    {
        public DateTime MessageRecored { get; set; }
        public DateTime? MessageListened { get; set; }
        public string RecordingUrl { get; set; }
        public string TranscriptionText { get; set; }
    }

    public class HomeController : Controller
    {
        private static readonly Dictionary<int, List<ApplicationMessage>> ApplicationMessagesByPlanningId =
            new Dictionary<int, List<ApplicationMessage>>();

        public ActionResult Analytics([Bind(Prefix = "id")]int planningId)
        {
            ViewBag.PlanningId = planningId;
            ViewBag.ApplicationMessages = ApplicationMessagesByPlanningId.ContainsKey(planningId)
                                              ? ApplicationMessagesByPlanningId[planningId]
                                              : new List<ApplicationMessage>();
            return View();
        }

        public ActionResult AnalyticsMessage([Bind(Prefix = "id1")]int planningId, [Bind(Prefix = "id2")]int index)
        {
            ViewBag.PlanningId = planningId;
            ViewBag.ApplicationMessage = ApplicationMessagesByPlanningId.ContainsKey(planningId) && ApplicationMessagesByPlanningId[planningId].Count > index
                                              ? ApplicationMessagesByPlanningId[planningId][index]
                                              : null;
            return ViewBag.ApplicationMessage == null ? (ActionResult)Redirect(string.Format("/planning/analytics/{0}", planningId)) : View();
        }

        public ActionResult Call()
        {
            return View();
        }

        public ActionResult GatherPlanner([Bind(Prefix = "Digits")]int planningId)
        {
            ViewBag.PlanningId = planningId;
            return View();
        }

        public ActionResult Recording([Bind(Prefix = "id")]int planningId, [Bind(Prefix = "RecordingUrl")]string recordingUrl)
        {
            if (!ApplicationMessagesByPlanningId.ContainsKey(planningId))
            {
                ApplicationMessagesByPlanningId.Add(planningId, new List<ApplicationMessage>());
            }
            ApplicationMessagesByPlanningId[planningId].Insert(0, new ApplicationMessage { MessageRecored = DateTime.Now, RecordingUrl = recordingUrl });
            ViewBag.PlanningId = planningId;
            return View();
        }

        public ActionResult Transcribe([Bind(Prefix = "id")]int planningId, [Bind(Prefix = "TranscriptionText")]string transcriptionText, [Bind(Prefix = "TranscriptionStatus")]string transcriptionStatus, [Bind(Prefix = "RecordingUrl")]string recordingUrl)
        {
            if (transcriptionStatus == "completed" && ApplicationMessagesByPlanningId.ContainsKey(planningId))
            {
                var applicationMessageForThisRecoringUrl =
                    ApplicationMessagesByPlanningId[planningId].FirstOrDefault(
                        applicationMessage => applicationMessage.RecordingUrl == recordingUrl);
                if (applicationMessageForThisRecoringUrl != null)
                {
                    applicationMessageForThisRecoringUrl.TranscriptionText = transcriptionText;
                }
            }
            return new EmptyResult();
        }

        public ActionResult GatherAgent([Bind(Prefix = "Digits")]int planningId)
        {
            var applicationMessages = ApplicationMessagesByPlanningId.ContainsKey(planningId)
                                          ? ApplicationMessagesByPlanningId[planningId]
                                          : new List<ApplicationMessage>();
            ViewBag.ApplicationMessages = applicationMessages;

            applicationMessages.Where(applicationMessage => !applicationMessage.MessageListened.HasValue).ForEach(
                applicationMessage => applicationMessage.MessageListened = DateTime.Now);

            return View();
        } 
    }
}
