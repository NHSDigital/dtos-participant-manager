Feature: Screening Eligibility and Navigation

  Scenario: User with P9 verification but not eligible for screening
    Given I am a logged-in user with a P9 level of verification
    When I access the application
    And I am not eligible for any screening
    Then I should see an informational page explaining my ineligibility

  Scenario: User with P9 verification and eligible for Breast Screening
    Given I am a logged-in user with a P9 level of verification
    And I am eligible for Breast Screening
    When I access the application
    Then I should see a Breast Screening link
    When I click on the Breast Screening link
    Then I should be taken to a page that shows my next test due date
    When I click the More Information link
    Then I should be redirected to the NHS UK webpage about mammograms

  Scenario: User with P9 verification and eligible for Cervical Screening
    Given I am a logged-in user with a P9 level of verification
    And I am eligible for Cervical Screening
    When I access the application
    Then I should see a Cervical Screening link
    When I click on the Cervical Screening link
    Then I should be taken to a page that shows my next test due date
    When I click the More Information link
    Then I should be redirected to the NHS UK webpage about cervical screening

  Scenario: User with P9 verification and eligible for both Breast and Cervical Screening
    Given I am a logged-in user with a P9 level of verification
    And I am eligible for both Breast and Cervical Screening
    When I access the application
    Then I should see both the Breast Screening link and the Cervical Screening link

  Scenario: User with P5 verification level
    Given I am a logged-in user with a P5 level of verification
    When I access the application
    Then I should be prompted to upgrade to P9 verification
