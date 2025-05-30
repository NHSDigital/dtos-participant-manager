Feature: Pages should not have any automatically detectable accessibility issues

    Scenario: Check for accessibility issues on the the homepage
        When I go to the page "/"
        Then I should expect "0" accessibility issues

    Scenario: Check for accessibility issues on the cookies policy page
        When I go to the page "/cookies-policy"
        Then I should expect "0" accessibility issues

    Scenario: Check for accessibility issues on the accessibility statement page
        When I go to the page "/accessibility-statement"
        Then I should expect "0" accessibility issues

    Scenario: Check for access accessibility issues on the screening page
        Given I am signed in as "onepathway" with password "Password123"
        Then I should expect "0" accessibility issues

    Scenario: Check for access accessibility issues on the breast screening page
        Given I am signed in as "onepathway" with password "Password123"
        And I go to the page "/breast-screening"
        Then I should expect "0" accessibility issues

    Scenario: Check for access accessibility issues on the cervical screening page
        Given I am signed in as "twopathway" with password "Password123"
        And I go to the page "/cervical-screening"
        Then I should expect "0" accessibility issues

        Scenario: Check for access accessibility issues on the bowel screening page
        Given I am signed in as "twopathway" with password "Password123"
        And I go to the page "/bowel-screening"
        Then I should expect "0" accessibility issues
