using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNet.Generators
{
    public class LiveGenerator : Generator
    {
        protected override string Map => "training2";
        private GameLayer GameLayer = new GameLayer("510c78d2-d786-41aa-b327-d6902d965217");

        protected override void ReGenerate()
        {
            var result = GameLayer.NewGame(Map);
            Vehicle = result.Vehicle;
            Packages = result.Dimensions.OrderBy(item => item.Id).ToList();
        }

        public override SubmitResponse Submit(List<PointPackage> solution)
        {
            return GameLayer.Submit(JsonSerializer.Serialize(solution), Map);
        }
    }
}
