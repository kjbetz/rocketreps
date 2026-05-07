# PostHog Events

Rocket Reps uses PostHog for product analytics when `PostHog:ProjectToken` is configured. Server-side events are captured through `RocketReps.Web/Analytics/IRocketRepsAnalytics`; the browser snippet in `RocketReps.Web/Components/App.razor` is rendered only in `Staging` and `Production`.

PostHog should help answer whether teachers can move from interest to classroom setup, whether classrooms get usable student/deck assignments, and whether students are practicing assigned work.

## Configuration

Set these values in deployed web environment files or local user secrets when analytics should be enabled:

```bash
PostHog__ProjectToken=phc_...
PostHog__HostUrl=https://us.i.posthog.com
```

`PostHog__HostUrl` is optional and defaults to `https://us.i.posthog.com`.

## Privacy Rules

Do not send student or teacher names, emails, usernames, generated passwords, card prompts, spelling words, typed answers, or free-form message text to PostHog.

Use internal IDs, role labels, status flags, counts, card types, and scheduling metadata instead. Audio-prompt cards should remain generic in analytics just like they are in student deck details.

## Event Summary

| Event | Emitted From | Purpose |
| --- | --- | --- |
| `early_access_requested` | `/early-access` form submit | Measures lead capture and plan interest. |
| `demo_launched` | `/demo/teacher`, `/demo/student` endpoints | Measures open-house demo usage by role. |
| `user_signed_in` | Login success | Measures authenticated usage by role and method. |
| `teacher_dashboard_viewed` | `/teacher` first interactive render | Measures teacher launchpad usage and account setup depth. |
| `classroom_created` | Classroom creation | Measures teacher activation into classroom setup. |
| `classroom_workspace_viewed` | `/teacher/classrooms/{id}` first interactive render | Measures classroom management engagement. |
| `student_login_created` | Student login generation | Measures roster setup progress. |
| `deck_assigned_to_classroom` | Classroom deck assignment | Measures whether teachers connect decks to classrooms. |
| `deck_assignment_toggled` | Active/inactive assignment toggle | Measures teacher control of student availability. |
| `teacher_deck_created` | Custom deck creation | Measures custom content creation starts. |
| `teacher_deck_publish_toggled` | Custom deck publish/unpublish | Measures custom deck readiness for assignment. |
| `teacher_deck_workspace_viewed` | `/teacher/decks/{id}` first interactive render | Measures custom deck editing engagement. |
| `teacher_deck_saved` | Custom deck metadata save | Measures deck maintenance. |
| `teacher_card_added` | Custom card add | Measures card authoring volume and card type mix. |
| `teacher_card_updated` | Custom card edit | Measures card maintenance. |
| `teacher_card_preview_opened` | Card preview open | Measures teacher validation before assignment. |
| `teacher_card_deleted` | Custom card delete | Measures content cleanup. |
| `student_dashboard_viewed` | `/student` first interactive render | Measures assigned student workload visibility. |
| `student_mission_details_opened` | Student mission details panel | Measures student interest in deck progress/details. |
| `review_session_started` | `/student/review/{assignmentId}` first interactive render | Measures review flow entry. |
| `review_answer_checked` | Student answer submission | Measures practice attempts, correctness, and scheduling outcomes. |
| `review_break_prompted` | 20-card checkpoint | Measures long review sessions. |
| `review_break_continued` | Continue after checkpoint | Measures willingness to keep practicing. |
| `review_session_ended` | End session confirmation | Measures manual session exits. |
| `review_session_completed` | No due or new cards remain | Measures students finishing available work. |

## Property Notes

Common properties added by `RocketRepsAnalytics`:

| Property | Meaning |
| --- | --- |
| `app` | Always `rocketreps`. |
| `environment` | ASP.NET Core environment name. |

Identity and funnel events:

| Event | Important Properties |
| --- | --- |
| `early_access_requested` | `role`, `plan_interest`, `has_school_or_organization`, `has_message`, `source` |
| `demo_launched` | `role`, `student_pool_size` for student demo launches |
| `user_signed_in` | `role`, `method`, `has_return_url` |

Teacher setup events:

| Event | Important Properties |
| --- | --- |
| `teacher_dashboard_viewed` | `classroom_count`, `student_count`, `active_deck_count`, `assigned_deck_count`, `stock_deck_count`, `published_teacher_deck_count` |
| `classroom_created` | `classroom_id`, `school_id` |
| `classroom_workspace_viewed` | `classroom_id`, `student_count`, `active_deck_count`, `assigned_deck_count`, `students_needing_attention`, `due_cards_now`, `recent_correct_percent` |
| `student_login_created` | `classroom_id`, `student_id`, `school_id`, `classroom_student_count` |
| `deck_assigned_to_classroom` | `classroom_id`, `deck_id`, `deck_card_count`, `assignment_id`, `is_active`, `was_existing_assignment` |
| `deck_assignment_toggled` | `classroom_id`, `assignment_id`, `deck_id`, `is_active` |

Custom deck events:

| Event | Important Properties |
| --- | --- |
| `teacher_deck_created` | `deck_id`, `subject`, `grade_band`, `is_published` |
| `teacher_deck_publish_toggled` | `deck_id`, `is_published`, `card_count` |
| `teacher_deck_workspace_viewed` | `deck_id`, `subject`, `grade_band`, `is_published`, `card_count` |
| `teacher_deck_saved` | `deck_id`, `subject`, `grade_band`, `is_published`, `card_count` |
| `teacher_card_added` | `deck_id`, `card_id`, `card_type`, `choice_count`, `deck_card_count`, `deck_is_published` |
| `teacher_card_updated` | `deck_id`, `card_id`, `card_type`, `choice_count`, `deck_card_count`, `deck_is_published` |
| `teacher_card_preview_opened` | `deck_id`, `card_id`, `card_type` |
| `teacher_card_deleted` | `deck_id`, `card_id`, `card_type`, `deck_card_count` |

Student practice events:

| Event | Important Properties |
| --- | --- |
| `student_dashboard_viewed` | `active_assignment_count`, `due_assignment_count`, `ready_assignment_count`, `caught_up_assignment_count`, `due_card_count`, `new_card_count` |
| `student_mission_details_opened` | `assignment_id`, `deck_id`, `subject`, `grade_band`, `card_count`, `due_card_count`, `new_card_count` |
| `review_session_started` | `assignment_id`, `deck_id`, `has_initial_card`, `initial_card_type`, `initial_card_is_due` |
| `review_answer_checked` | `assignment_id`, `deck_id`, `card_id`, `card_type`, `is_due_card`, `was_correct`, `rating`, `completed_count`, `scheduled_interval_minutes`, `next_fsrs_state` |
| `review_break_prompted` | `assignment_id`, `deck_id`, `completed_count` |
| `review_break_continued` | `assignment_id`, `deck_id`, `completed_count` |
| `review_session_ended` | `assignment_id`, `deck_id`, `completed_count`, `had_pending_card` |
| `review_session_completed` | `assignment_id`, `deck_id`, `completed_count` |

## Browser Snippet

The browser snippet is useful for PostHog web analytics, pageview/session properties, surveys, toolbar support, and browser-side feature flag behavior. It is not required for the server-side workflow events above.

The snippet uses `person_profiles: 'identified_only'`. Do not change this to create anonymous person profiles unless there is a clear product need and the privacy impact has been reviewed.
