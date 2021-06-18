using System;

namespace SoftDeleteDemo.Entities
{
    public class DemoModel : GenericModel, ISoftDelete
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
    }
}