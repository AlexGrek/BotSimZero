using Stride.Engine;
using Stride.Games;

namespace BotSimZero.Entities
{
    public class SmartEntityProcessor: EntityProcessor<SmartEntity>
    {
        

        public override void Update(GameTime time)
        {
            foreach (var myComponent in ComponentDatas.Values)
            {
                //Console.WriteLine($"myComponent with value {myComponent.MyValue} at {time.Total.TotalSeconds}");
            }
        }
    }
}
