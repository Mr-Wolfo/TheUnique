namespace TheUnique.Core.Interaction
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(Player.PlayerEntity player);
        float GetInteractionDistance();
    }
}