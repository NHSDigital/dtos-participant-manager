Feature: I can see the Homepage

    Scenario: Check for page heading
        When I go to the page "/"
        Then I see the heading "Manage your screening"

    Scenario: Check for the NHS login button
        When I go to the page "/"
        Then I see the button "Continue to NHS login"
