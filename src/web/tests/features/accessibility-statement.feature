Feature: I can see the Accessibility statement page

    Scenario: Check for page heading
        When I go to the page "/accessibility-statement"
        Then I see the heading "Accessibility statement"
