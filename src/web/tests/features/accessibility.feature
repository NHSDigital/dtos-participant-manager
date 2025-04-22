Feature: Pages should not have any automatically detectable accessibility issues

    Scenario: Check for accessibility issues on the Homepage
        When I go to the page "/"
        Then I should expect "0" accessibility issues

    Scenario: Check for accessibility issues on Cookies policy page
        When I go to the page "/cookies-policy"
        Then I should expect "0" accessibility issues

    Scenario: Check for accessibility issues on Accessibility statement page
        When I go to the page "/accessibility-statement"
        Then I should expect "0" accessibility issues
