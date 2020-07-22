using System.Collections.Generic;

namespace SkillMatcher
{
    public interface IInteractionMonitor
    {
        void RegisterUser(int userId);
        void RegisterInteraction(int userId1, int userId2);
        List<int> GetInteractionGroup(int userId);
        List<List<int>> GetAllInteractionGroups();
    }
}