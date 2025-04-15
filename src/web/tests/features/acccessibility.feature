Feature: Pages should not have any automatically detectable accessibility issues

    Scenario: Check for accessibility issues on the Homepage
        Given I am on the page "/"
        When I arrive on on the page "/"
        Then I should expect "0" accessibility issues

    Scenario: Check for accessibility issues on Cookies policy page
        Given I am on the page "/cookies-policy"
        When I arrive on on the page "/cookies-policy"
        Then I should expect "0" accessibility issues

    Scenario: Check for accessibility issues on Accessibility statement page
        Given I am on the page "/accessibility-statement"
        When I arrive on on the page "/accessibility-statement"
        Then I should expect "0" accessibility issues
