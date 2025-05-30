Feature: As an unauthenticated user I cannot access protected pages

    Scenario: Check if I am redirected from the protected My screening page
        When I go to the page "/screening"
        Then I see the heading "Manage your screening"

    Scenario: Check if I am redirected from the protected Breast screening page
        When I go to the page "/breast-screening"
        Then I see the heading "Manage your screening"

    Scenario: Check if I am redirected from the protected Bowel screening page
        When I go to the page "/bowel-screening"
        Then I see the heading "Manage your screening"

    Scenario: Check if I am redirected from the protected Cervical screening page
        When I go to the page "/cervical-screening"
        Then I see the heading "Manage your screening"
