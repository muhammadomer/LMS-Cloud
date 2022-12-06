using Database.Entities;
using LogApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DAL
{
    public class ActivityLogDAL
    {
        SinglePoint_CloudEntities SinglePoint_CloudEntities = new SinglePoint_CloudEntities();
        public List<ActivityLogEntity> GetActivityLogs()
        {
            try
            {
                var activities = (from al in SinglePoint_CloudEntities.ActivityLog 
                                  from u in SinglePoint_CloudEntities.Users.Where(x => x.UserId == al.UserId)
                                  orderby al.ActivityTime descending
                                  select new ActivityLogEntity
                                  {
                                      ActivityDetail = al.ActivityDetail,
                                      ActivityTime = al.ActivityTime.ToString(),
                                      FullName = u.UserFirstName + " " + u.UserLastName

                                  }
                                  ).ToList();

                //.Where(x => x.UserId == al.UserId)

                return activities;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return null;
            }
        }
        public static void LogActivity(string activityDetail)
        {
            try
            {
                //Log4Net.WriteLog(activityDetail, LogType.GENERALLOG);
                //SinglePoint_CloudEntities cloudEntities = new SinglePoint_CloudEntities();
                //var LoggedInUser = UserDAL.GetCurrentUser();
                //ActivityLog activity = new ActivityLog();
                //activity.UserId = LoggedInUser.UserId;
                //activity.ActivityTime = DateTime.Now;
                //activity.ActivityDetail = activityDetail;
                //cloudEntities.ActivityLog.Add(activity);
                //cloudEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
            }
        }
    }
}
