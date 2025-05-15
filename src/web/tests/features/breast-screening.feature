Feature: Breast Screening Page

  Scenario: One pathway user logs in
    Given I am signed in as "onepathway" with password "Password123"
    When I click the card with title "Breast Screening"
    Then I should be redirected to the page "/breast-screening"
    And I should see a card with title "About Breast Screening"
    And I should see the user name "One" in the header
