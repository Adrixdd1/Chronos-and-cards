using UnityEngine;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Definición base de un ítem consumible.
    /// Cada ScriptableObject representa un tipo de ítem con su efecto asociado.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "ChronosAndCards/ItemDefinition")]
    public class ItemDefinition : ScriptableObject, IItem
    {
        [SerializeField] private string _name;
        [SerializeField, TextArea(2, 4)] private string _description;
        [SerializeField] private ItemActivationPhase _phase;
        [SerializeField] private Sprite _icon;

        public string Name => _name;
        public string Description => _description;
        public ItemActivationPhase Phase => _phase;
        public Sprite Icon => _icon;

        /// <summary>
        /// Referencia al efecto. Se asigna desde la factory o via SerializeReference.
        /// </summary>
        public virtual IItemEffect Effect { get; protected set; }
    }
}
