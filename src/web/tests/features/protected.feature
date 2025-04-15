Feature: As an unauthenticated user I cannot access protected pages

    Scenario: Check if I am redirected from the protected My screening page
        Given I am on the page "/screening"
        When I arrive on on the page "/"
        Then I see the heading "Manage your screening"

    Scenario: Check if I am redirected from the protected Breast screening page
        Given I am on the page "/breast-screening"
        When I arrive on on the page "/"
        Then I see the heading "Manage your screening"

    Scenario: Check if I am redirected from the protected Bowel screening page
        Given I am on the page "/bowel-screening"
        When I arrive on on the page "/"
        Then I see the heading "Manage your screening"

    Scenario: Check if I am redirected from the protected Cervical screening page
        Given I am on the page "/cervical-screening"
        When I arrive on on the page "/"
        Then I see the heading "Manage your screening"
