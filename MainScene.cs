using Godot;
using System;

using GodotArray = Godot.Collections.Array;

public class MainScene : Control
{
    private InkStory story;
    private ScrollContainer scroll;
    private BoxContainer container;

    private Timer timer;

    public override void _Ready()
    {
        // Retrieve or create some Nodes we know we'll need quite often
        story = GetNode<InkStory>("Story");
        scroll = GetNode<ScrollContainer>("VMargin/Scroll");
        container = GetNode<BoxContainer>("VMargin/Scroll/HMargin/StoryContainer");
        timer = new Timer();
        timer.Autostart = false;
        timer.WaitTime = 0.3f;
        timer.OneShot = true;
        timer.Connect("timeout", story, "Continue");
        AddChild(timer);

        // Start the story
        timer.Start();
    }

    public void OnStoryInkEnded()
    {
        AddToContainer(new HSeparator(), 0.4f);
        AddToContainer(new HSeparator(), 0.5f);
        AddToContainer(new HSeparator(), 0.6f);
    }

    public void OnStoryInkContinued(String text, String[] tags)
    {
        text = text.Trim();
        if (text.Length > 0)
            AddToContainer(CreateLabel(text));

        // Go again
        timer.Start();
    }

    public void OnStoryInkChoices(String[] choices)
    {
        AddToContainer(new HSeparator(), 0.2f);
        // Add a button for each choice
        for (int i = 0; i < choices.Length; ++i)
            AddToContainer(CreateButton(choices[i], i), 0.4f);
    }

    protected void OnChoiceClick(int choiceIndex)
    {
        foreach (Node choice in GetTree().GetNodesInGroup("choiceButtons"))
            choice.QueueFree();
        // Choose the clicked choice and continue onward
        story.ChooseChoiceIndexAndContinue(choiceIndex);
    }

    private Label CreateLabel(String text)
    {
        Label label = new Label();
        label.Autowrap = true;
        label.Text = text;
        return label;
    }

    private Button CreateButton(String text, int choiceIndex)
    {
        Button button = new Button();
        button.Text = text;
        button.SizeFlagsHorizontal = (int)SizeFlags.ShrinkCenter;
        button.AddToGroup("choiceButtons");
        button.Connect("pressed", this, "OnChoiceClick", new GodotArray { choiceIndex });
        return button;
    }

    private async void AddToContainer(CanvasItem newNode, float delay = 0f)
    {
        // Fade-in effect
        Tween tween = new Tween();
        tween.InterpolateProperty(newNode, "modulate", Colors.Transparent, Colors.White, 0.5f,
                                  Tween.TransitionType.Linear, Tween.EaseType.InOut, delay);
        tween.Connect("tween_all_completed", tween, "queue_free");
        newNode.AddChild(tween);
        newNode.Modulate = Colors.Transparent;
        container.AddChild(newNode); // We actually add the thing here
        tween.Start();
 
        // Wait for the scrollbar to recalculate and scroll to the bottom
        await ToSignal(GetTree(), "idle_frame");
        scroll.ScrollVertical = (int)scroll.GetVScrollbar().MaxValue;
    }
}
