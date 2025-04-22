Feature: Cervical Screening Page

  Scenario: One pathway user logs in
    Given I am signed in as "twopathway" with password "Password123"
    When I click the card with title "Cervical Screening"
    Then I should be redirected to the page "/cervical-screening"
    And I should see a card with title "About Cervical Screening"
    And I should see the user name "Two" in the header
