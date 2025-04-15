Feature: Can I see the homepage

    Scenario: Check for page heading
        Given I am on the Homepage "/"
        When I arrive on on the Homepage "/"
        Then I see the heading "Manage your screening"

    Scenario: Check for the NHS login button
       Given I am on the Homepage "/"
       When I arrive on on the Homepage "/"
       Then I see the button "Continue to NHS login"
