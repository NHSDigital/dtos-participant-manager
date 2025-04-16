Feature: I can see the Accessibility statement page

    Scenario: Check for page heading
        Given I am on the page "/accessibility-statement"
        When I arrive on on the page "/accessibility-statement"
        Then I see the heading "Accessibility statement"
