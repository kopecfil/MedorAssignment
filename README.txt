

Comments about the assignment and solution
    The description says that the data from the coindesk api should be refreshed regularly.
        That is problematic because either we can have the data refresh itself, or we can have it editable by the user.
        If we would automatically refresh the data, we would lose the user input every time and that would be awful UX.
        In order to preserve the functionality of editing the data, saving and loading it, I'm going to make it do that and not do any automatic updates of the data.
        It would be possible to make automatic updates something that the user can toggle on and off (it could be smart and toggle automatic updates off when the user makes changes), but but that is outside of the original scope of the assignment and would need to be discussed and agreed upon before implementation.
         