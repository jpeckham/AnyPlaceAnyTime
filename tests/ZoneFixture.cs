using EFT;
using EFT.Quests;
using Il2CppInterop.Runtime;
namespace AnyPlaceAnyTimeChecks;
internal static class ZoneFixture
{
    internal static QuestBook Create()
    {
        const string id="aaaaaaaaaaaaaaaaaaaaaaaa";
        var counters=new Il2CppSystem.Collections.Generic.Dictionary<MongoID,TaskConditionCounter>();
        var book=new QuestBook(id,EPlayerSide.Usec,new Il2CppSystem.Collections.Generic.List<QuestStatusData>(),counters,(EProfileType)0,true,false);
        var conditions=new ConditionCollection();
        var tool=new ConditionLeaveItemAtLocation { id=new MongoID("aaaaaaaaaaaaaaaaaaaaaaa1"),target=new[]{"590c2e1186f77425357b6124"},zoneIds=new[]{"anyplace-test-zone"},value=1,VisibilityConditions=Array.Empty<Condition>() };
        var task=new ConditionLeaveItemAtLocation { id=new MongoID("aaaaaaaaaaaaaaaaaaaaaaa2"),target=new[]{"591092ef86f7747bb8703422"},zoneIds=new[]{"anyplace-test-zone"},value=1,VisibilityConditions=Array.Empty<Condition>() };
        var repeater=new ConditionPlaceBeacon { id=new MongoID("aaaaaaaaaaaaaaaaaaaaaaa3"),target=new[]{"63a0b2eabea67a6d93009e52"},zoneIds=new[]{"anyplace-test-zone"},value=1,VisibilityConditions=Array.Empty<Condition>() };
        conditions.Add(tool);conditions.Add(task);conditions.Add(repeater);
        var groups=new ConditionsDict();groups.Add(EQuestStatus.AvailableForFinish,conditions);
        var template=new QuestTemplate(id) { Conditions=groups };
        var status=new QuestStatusData { Id=id,Status=EQuestStatus.Started,CompletedConditions=new Il2CppSystem.Collections.Generic.HashSet<MongoID>(),StatusStartTimestamps=new Il2CppSystem.Collections.Generic.Dictionary<EQuestStatus,double>() };
        var quest=new Quest(template,status,counters,false,false);
        foreach(var condition in new Condition[]{tool,task,repeater}) {
            var checker=new ConditionProgressChecker(condition);
            checker.SetCurrentValueGetter((Il2CppSystem.Func<ConditionProgressChecker,double>)(_=>0d));
            quest.ProgressCheckers[condition]=checker;
        }
        book.Add(quest);
        return book;
    }
}
