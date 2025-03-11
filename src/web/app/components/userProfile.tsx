import SignOutButton from "@/app/components/signOutButton";

interface UserProfileProps {
  readonly firstName?: string;
  readonly lastName?: string;
  readonly nhsNumber?: string;
}

export default async function UserProfile({
  firstName,
  lastName,
  nhsNumber,
}: Readonly<UserProfileProps>) {
  return (
    <>
      <hr />
      <p>
        Logged in as {firstName} {lastName} ({nhsNumber})
      </p>
      <SignOutButton />
    </>
  );
}
