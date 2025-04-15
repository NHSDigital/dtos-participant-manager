Feature: Homepage

    Scenario: Check title
        Given I am on the Homepage "/"
        When I arrive on on the Homepage "/"
        Then I see the heading "Manage your screening"
